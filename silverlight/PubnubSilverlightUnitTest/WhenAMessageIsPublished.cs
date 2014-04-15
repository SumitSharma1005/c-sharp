﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Silverlight.Testing;
using PubNubMessaging.Core;

namespace PubnubSilverlight.UnitTest
{
    [TestClass]
    public class WhenAMessageIsPublished : SilverlightTest
    {
        bool isPublished2 = false;
        bool isPublished3 = false;

        bool isUnencryptPublished = false;
        bool isUnencryptObjectPublished = false;
        bool isEncryptObjectPublished = false;
        bool isUnencryptDH = false;
        bool isUnencryptObjectDH = false;
        bool isEncryptObjectDH = false;
        bool isEncryptPublished = false;
        bool isSecretEncryptPublished = false;
        bool isEncryptDH = false;
        bool isSecretEncryptDH = false;
        bool isComplexObjectPublished = false;
        bool isComplexObjectDetailedHistory = false;
        bool isSerializedObjectMessagePublished = false;
        bool isSerializedObjectMessageDetailedHistory = false;
        bool isLargeMessagePublished = false;

        long unEncryptPublishTimetoken = 0;
        long unEncryptObjectPublishTimetoken = 0;
        long encryptObjectPublishTimetoken = 0;
        long encryptPublishTimetoken = 0;
        long secretEncryptPublishTimetoken = 0;
        long complexObjectPublishTimetoken = 0;
        long serializedMessagePublishTimetoken = 0;

        ManualResetEvent mrePublish = new ManualResetEvent(false);
        ManualResetEvent mreDetailedHistory = new ManualResetEvent(false);

        const string messageForUnencryptPublish = "Pubnub Messaging API 1";
        const string messageForEncryptPublish = "漢語";
        const string messageForSecretEncryptPublish = "Pubnub Messaging API 2";
        const string messageLarge2K = "Numerous questions remain about the origins of the chemical and what impact its apparent use could have on the ongoing Syrian civil war and international involvement in it.When asked if the intelligence community's conclusion pushed the situation across President Barack Obama's \"red line\" that could potentially trigger more U.S. involvement in the Syrian civil war, Hagel said it's too soon to say.\"We need all the facts. We need all the information,\" he said. \"What I've just given you is what our intelligence community has said they know. As I also said, they are still assessing and they are still looking at what happened, who was responsible and the other specifics that we'll need.\" In a letter sent to lawmakers before Hagel's announcement, the White House said that intelligence analysts have concluded \"with varying degrees of confidence that the Syrian regime has used chemical weapons on a small scale in Syria, specifically the chemical agent sarin.\" In the letter, signed by White House legislative affairs office Director Miguel Rodriguez, the White House said the \"chain of custody\" of the chemicals was not clear and that intelligence analysts could not confirm the circumstances under which the sarin was used, including the role of Syrian President Bashar al-Assad's regime. Read Rodriguez's letter to Levin (PDF) But, the letter said, \"we do believe that any use of chemical weapons in Syria would very likely have originated with the Assad regime.\" The Syrian government has been battling a rebellion for more than two years, bringing international condemnation of the regime and pleas for greater international assistance. The United Nations estimated in February that more than 70,000 people had died since the conflict began. The administration is \"pressing for a comprehensive United Nations investigation that can credibly evaluate the evidence and establish what took place,\" the White House letter said. Sen. John McCain, one of the lawmakers who received the letter, said the use of";
        string messageObjectForUnencryptPublish = "";
        string messageObjectForEncryptPublish = "";
        string messageComplexObjectForPublish = "";
        string serializedObjectMessageForPublish;

        bool receivedGrantMessage = false;
        bool grantInitCallbackInvoked = false;

        [ClassInitialize, Asynchronous]
        public void Init()
        {
            if (!PubnubCommon.PAMEnabled)
            {
                EnqueueTestComplete();
                return;
            }

            receivedGrantMessage = false;

            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, PubnubCommon.SecretKey, "", false);

            PubnubUnitTest unitTest = new PubnubUnitTest();
            unitTest.TestClassName = "GrantRequestUnitTest";
            unitTest.TestCaseName = "Init";
            pubnub.PubnubUnitTest = unitTest;

            string channel = "hello_my_channel";

            EnqueueCallback(() => pubnub.GrantAccess<string>(channel, true, true, 20, ThenPublishInitializeShouldReturnGrantMessage, DummyErrorCallback));
            //Thread.Sleep(1000);

            EnqueueConditional(() => grantInitCallbackInvoked);

            EnqueueCallback(() => Assert.IsTrue(receivedGrantMessage, "WhenAClientIsPresent Grant access failed."));

            EnqueueTestComplete();
        }

        [Asynchronous]
        void ThenPublishInitializeShouldReturnGrantMessage(string receivedMessage)
        {
            try
            {
                if (!string.IsNullOrEmpty(receivedMessage) && !string.IsNullOrEmpty(receivedMessage.Trim()))
                {
                    object[] serializedMessage = JsonConvert.DeserializeObject<object[]>(receivedMessage);
                    JContainer dictionary = serializedMessage[0] as JContainer;
                    var status = dictionary["status"].ToString();
                    if (status == "200")
                    {
                        receivedGrantMessage = true;
                    }
                }
            }
            catch { }
            finally
            {
                grantInitCallbackInvoked = true;
            }
        }

        [TestMethod, Asynchronous]
        public void ThenUnencryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";
                    string message = messageForUnencryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenUnencryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptPublishCodeCallback, DummyErrorCallback));
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isUnencryptPublished)
                        {
                            Assert.IsTrue(isUnencryptPublished, "Unencrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, unEncryptPublishTimetoken, -1, false, CaptureUnencryptDetailedHistoryCallback, DummyErrorCallback);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isUnencryptDH, "Unable to match the successful unencrypt Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessUnencryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isUnencryptPublished = true;
                        unEncryptPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureUnencryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jObj = JArray.Parse(receivedObj[0].ToString());
                    if (jObj[0].ToString() == messageForUnencryptPublish)
                    {
                        isUnencryptDH = true;
                    }
                }
            }

            //isUnencryptCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isUnencryptObjectPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    string channel = "hello_my_channel";

                    object message = new CustomClass();
                    messageObjectForUnencryptPublish = JsonConvert.SerializeObject(message);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenUnencryptObjectPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessUnencryptObjectPublishCodeCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isUnencryptObjectPubCheck);
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isUnencryptObjectPublished)
                        {
                            Assert.IsTrue(isUnencryptObjectPublished, "Unencrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, unEncryptObjectPublishTimetoken, -1, false, CaptureUnencryptObjectDetailedHistoryCallback, DummyErrorCallback);
                            //EnqueueConditional(() => isUnencryptObjectDHCheck);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isUnencryptObjectDH, "Unable to match the successful unencrypt object Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessUnencryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isUnencryptObjectPublished = true;
                        unEncryptObjectPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            //isUnencryptObjectPubCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureUnencryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr[0].ToString(Formatting.None) == messageObjectForUnencryptPublish)
                    {
                        isUnencryptObjectDH = true;
                    }
                }
            }

            //isUnencryptObjectDHCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptObjectPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
                    string channel = "hello_my_channel";
                    object message = new SecretCustomClass();
                    messageObjectForEncryptPublish = JsonConvert.SerializeObject(message);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenEncryptObjectPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessEncryptObjectPublishCodeCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isEncryptObjectPubCheck);
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isEncryptObjectPublished)
                        {
                            Assert.IsTrue(isEncryptObjectPublished, "Encrypt Object Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, encryptObjectPublishTimetoken, -1, false, CaptureEncryptObjectDetailedHistoryCallback, DummyErrorCallback);
                            //EnqueueConditional(() => isEncryptObjectDHCheck);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isEncryptObjectDH, "Unable to match the successful encrypt object Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessEncryptObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isEncryptObjectPublished = true;
                        encryptObjectPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            //isEncryptObjectPubCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureEncryptObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr[0].ToString(Formatting.None) == messageObjectForEncryptPublish)
                    {
                        isEncryptObjectDH = true;
                    }
                }
            }

            //isEncryptObjectDHCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isEncryptPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "enigma", false);
                    string channel = "hello_my_channel";
                    string message = messageForEncryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenEncryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessEncryptPublishCodeCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isEncryptPubCheck);
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isEncryptPublished)
                        {
                            Assert.IsTrue(isEncryptPublished, "Encrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, encryptPublishTimetoken, -1, false, CaptureEncryptDetailedHistoryCallback, DummyErrorCallback);
                            //EnqueueConditional(() => isEncryptDHCheck);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isEncryptDH, "Unable to decrypt the successful Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isEncryptPublished = true;
                        encryptPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            //isEncryptPubCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        public void CaptureEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr[0].ToString() == messageForEncryptPublish)
                    {
                        isEncryptDH = true;
                    }
                }
            }

            //isEncryptDHCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo()
        {
            isSecretEncryptPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "key", "enigma", false);
                    string channel = "hello_my_channel";
                    string message = messageForSecretEncryptPublish;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenSecretKeyWithEncryptPublishShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessSecretEncryptPublishCodeCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isSecretEncryptPubCheck);
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                    {
                        if (!isSecretEncryptPublished)
                        {
                            Assert.IsTrue(isSecretEncryptPublished, "Secret Encrypt Publish Failed");
                        }
                        else
                        {
                            pubnub.DetailedHistory<string>(channel, -1, secretEncryptPublishTimetoken, -1, false, CaptureSecretEncryptDetailedHistoryCallback, DummyErrorCallback);
                            //EnqueueConditional(() => isSecretEncryptDHCheck);
                            mreDetailedHistory.WaitOne(310 * 1000);
                            Assert.IsTrue(isSecretEncryptDH, "Unable to decrypt the successful Secret key Publish");
                        }
                    });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSuccessSecretEncryptPublishCodeCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isSecretEncryptPublished = true;
                        secretEncryptPublishTimetoken = Convert.ToInt64(receivedObj[2].ToString());
                    }
                }
            }
            //isSecretEncryptPubCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        private void CaptureSecretEncryptDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    JArray jArr = JArray.Parse(receivedObj[0].ToString());
                    if (jArr[0].ToString() == messageForSecretEncryptPublish)
                    {
                        isSecretEncryptDH = true;
                    }
                }
            }

            //isSecretEncryptDHCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod]
        public void ThenPubnubShouldGenerateUniqueIdentifier()
        {
            Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

            Assert.IsNotNull(pubnub.GenerateGuid());
        }

        [TestMethod]
        [ExpectedException(typeof(MissingFieldException))]
        public void ThenPublishKeyShouldNotBeEmpty()
        {
            Pubnub pubnub = new Pubnub("", PubnubCommon.SubscribeKey, "", "", false);

            string channel = "hello_my_channel";
            string message = "Pubnub API Usage Example";

            pubnub.Publish<string>(channel, message, null, DummyErrorCallback);
        }

        [TestMethod, Asynchronous]
        public void ThenOptionalSecretKeyShouldBeProvidedInConstructor()
        {
            isPublished2 = false;
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "key");
                    string channel = "hello_my_channel";
                    string message = "Pubnub API Usage Example";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenOptionalSecretKeyShouldBeProvidedInConstructor";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSecretKeyPublishCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isCheck2);
                    mrePublish.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(isPublished2, "Publish Failed with secret key"));

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnSecretKeyPublishCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isPublished2 = true;
                    }
                }
            }
            //isCheck2 = true;
            mrePublish.Set();
        }

        [TestMethod, Asynchronous]
        public void IfSSLNotProvidedThenDefaultShouldBeFalse()
        {
            isPublished3 = false;
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "");
                    string channel = "hello_my_channel";
                    string message = "Pubnub API Usage Example";

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "IfSSLNotProvidedThenDefaultShouldBeFalse";
                    pubnub.PubnubUnitTest = unitTest;

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnNoSSLDefaultFalseCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isCheck3);
                    mrePublish.WaitOne(310 * 1000);
                    EnqueueCallback(() => Assert.IsTrue(isPublished3, "Publish Failed with no SSL"));

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        public void ReturnNoSSLDefaultFalseCallback(string result)
        {
            if (!string.IsNullOrWhiteSpace(result))
            {
                object[] receivedObj = JsonConvert.DeserializeObject<object[]>(result);
                if (receivedObj is object[])
                {
                    long statusCode = Int64.Parse(receivedObj[0].ToString());
                    string statusMsg = (string)receivedObj[1];
                    if (statusCode == 1 && statusMsg.ToLower() == "sent")
                    {
                        isPublished3 = true;
                    }
                }
            }
            //isCheck3 = true;
            mrePublish.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo()
        {
            isComplexObjectPublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenComplexMessageObjectShouldReturnSuccessCodeAndInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    object message = new PubnubDemoObject();
                    messageComplexObjectForPublish = JsonConvert.SerializeObject(message);

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessComplexObjectPublishCodeCallback, DummyErrorCallback));
                    //EnqueueConditional(() => isComplexObjectPublishCheck);
                    mrePublish.WaitOne(310 * 1000);

                    EnqueueCallback(() =>
                        {
                            if (!isComplexObjectPublished)
                            {
                                Assert.IsTrue(isComplexObjectPublished, "Complex Object Publish Failed");
                            }
                            else
                            {
                                pubnub.DetailedHistory<string>(channel, -1, complexObjectPublishTimetoken, -1, false, CaptureComplexObjectDetailedHistoryCallback, DummyErrorCallback);
                                //EnqueueConditional(() => isComplexObjectDetailedHistoryCheck);
                                mreDetailedHistory.WaitOne(310 * 1000);
                                Assert.IsTrue(isComplexObjectDetailedHistory, "Unable to match the successful unencrypt object Publish");
                            }
                        });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        private void ReturnSuccessComplexObjectPublishCodeCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isComplexObjectPublished = true;
                        complexObjectPublishTimetoken = Convert.ToInt64(deserializedMessage[2].ToString());
                    }
                }
            }

            //isComplexObjectPublishCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        private void CaptureComplexObjectDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString(Formatting.None) == messageComplexObjectForPublish)
                    {
                        isComplexObjectDetailedHistory = true;
                    }
                }
            }

            //isComplexObjectDetailedHistoryCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenDisableJsonEncodeShouldSendSerializedObjectMessage()
        {
            isSerializedObjectMessagePublished = false;
            mrePublish = new ManualResetEvent(false);
            mreDetailedHistory = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", false);
                    pubnub.EnableJsonEncodingForPublish = false;

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenDisableJsonEncodeShouldSendSerializedObjectMessage";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    object message = "{\"operation\":\"ReturnData\",\"channel\":\"Mobile1\",\"sequenceNumber\":0,\"data\":[\"ping 1.0.0.1\"]}";
                    serializedObjectMessageForPublish = message.ToString();

                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, ReturnSuccessSerializedObjectMessageForPublishCallback, DummyErrorCallback));
                    mrePublish.WaitOne(310 * 1000);
                    //EnqueueConditional(() => isSerializedObjectMessageCheck);

                    EnqueueCallback(() =>
                        {
                            if (!isSerializedObjectMessagePublished)
                            {
                                EnqueueCallback(() => Assert.IsTrue(isSerializedObjectMessagePublished, "Serialized Object Message Publish Failed"));
                            }
                            else
                            {
                                pubnub.DetailedHistory<string>(channel, -1, serializedMessagePublishTimetoken, -1, false, CaptureSerializedMessagePublishDetailedHistoryCallback, DummyErrorCallback);
                                //EnqueueConditional(() => isSerializedMessageDetailedHistoryCheck);
                                mreDetailedHistory.WaitOne(310 * 1000);
                                Assert.IsTrue(isSerializedObjectMessageDetailedHistory, "Unable to match the successful serialized object message Publish");
                            }
                        });

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        private void ReturnSuccessSerializedObjectMessageForPublishCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedResult = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedResult is object[])
                {
                    long statusCode = Int64.Parse(deserializedResult[0].ToString());
                    string statusMessage = (string)deserializedResult[1];
                    if (statusCode == 1 && statusMessage.ToLower() == "sent")
                    {
                        isSerializedObjectMessagePublished = true;
                        serializedMessagePublishTimetoken = Convert.ToInt64(deserializedResult[2].ToString());
                    }
                }
            }
            //isSerializedObjectMessageCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        private void CaptureSerializedMessagePublishDetailedHistoryCallback(string result)
        {
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrEmpty(result.Trim()))
            {
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(result);
                if (deserializedMessage is object[])
                {
                    JArray message = deserializedMessage[0] as JArray;
                    if (message != null && message[0].ToString(Formatting.None) == serializedObjectMessageForPublish)
                    {
                        isSerializedObjectMessageDetailedHistory = true;
                    }
                }
            }

            //isSerializedMessageDetailedHistoryCheck = true;
            mreDetailedHistory.Set();
        }

        [TestMethod, Asynchronous]
        public void ThenLargeMessageShoudFailWithMessageTooLargeInfo()
        {
            isLargeMessagePublished = false;
            mrePublish = new ManualResetEvent(false);

            ThreadPool.QueueUserWorkItem((s) =>
                {
                    Pubnub pubnub = new Pubnub(PubnubCommon.PublishKey, PubnubCommon.SubscribeKey, "", "", true);

                    PubnubUnitTest unitTest = new PubnubUnitTest();
                    unitTest.TestClassName = "WhenAMessageIsPublished";
                    unitTest.TestCaseName = "ThenLargeMessageShoudFailWithMessageTooLargeInfo";
                    pubnub.PubnubUnitTest = unitTest;

                    string channel = "hello_my_channel";
                    string message = messageLarge2K.Substring(0, 1320);
                    EnqueueCallback(() => pubnub.Publish<string>(channel, message, DummyPublishMessageTooLargeInfoCallback, PublishMessageTooLargeErrorCallback));
                    mrePublish.WaitOne(310 * 100);
                    //EnqueueConditional(() => isPublishMessageTooLargeCheck);
                    EnqueueCallback(() => Assert.IsTrue(isLargeMessagePublished, "Message Too Large is not failing as expected."));

                    EnqueueTestComplete();
                });
        }

        [Asynchronous]
        private void DummyPublishMessageTooLargeInfoCallback(string result)
        {
        }

        [Asynchronous]
        private void PublishMessageTooLargeErrorCallback(PubnubClientError result)
        {
            if (result != null && result.StatusCode > 0)
            {
                string message = result.Message;
                object[] deserializedMessage = JsonConvert.DeserializeObject<object[]>(message);
                if (deserializedMessage is object[])
                {
                    long statusCode = Int64.Parse(deserializedMessage[0].ToString());
                    string statusMessage = (string)deserializedMessage[1];
                    if (statusCode == 0 && statusMessage.ToLower() == "message too large")
                    {
                        isLargeMessagePublished = true;
                    }
                }
            }
            //isPublishMessageTooLargeCheck = true;
            mrePublish.Set();
        }

        [Asynchronous]
        private void DummyErrorCallback(PubnubClientError result)
        {
        }

    }
}
