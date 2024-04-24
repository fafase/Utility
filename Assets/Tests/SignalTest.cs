using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tools;
using UnityEngine;
using UnityEngine.TestTools;

public class SignalTest
{
    private int m_callNoParams;
    private string m_data;
    [SetUp]
    public void Setup() 
    {
        Signal.ClearAll();
        m_callNoParams = 0;
        m_data = null;
    }


    [Test]
    public void SignalConnectNoParamTest()
    {
        Signal.Connect<SignalDataNoParamA>(()=> { });
        Signal.Connect<SignalDataNoParamA>(MethodNoParams);
        int count = Signal.GetCount<SignalDataNoParamA>();
        Assert.AreEqual(2, count);
    }

    [Test]
    public void SignalConnectParamsTest()
    {
        Signal.Connect<SignalDataParamA>(MethodParamsA);
        int count = Signal.GetCount<SignalDataParamA>();
        Assert.AreEqual(1, count);
        Signal.Connect<SignalDataParamA>(MethodParamsB);
        count = Signal.GetCount<SignalDataParamA>();
        Assert.AreEqual(2, count);
    }
    [Test]
    public void SignalClearTest()
    {
        Signal.Connect<SignalDataNoParamA>(() => { });
        Signal.Connect<SignalDataNoParamA>(MethodNoParams);
        int count = Signal.GetCount<SignalDataNoParamA>();
        Assert.AreEqual(2, count);
        Signal.ClearList<SignalDataNoParamA>();
        count = Signal.GetCount<SignalDataNoParamA>();
        Assert.AreEqual(0, count);
    }
    [Test]
    public void SignalRemoveTest() 
    {
        Signal.Connect<SignalDataNoParamA>(() => { });
        Signal.Connect<SignalDataNoParamA>(MethodNoParams);
        int count = Signal.GetCount<SignalDataNoParamA>();
        Assert.AreEqual(2, count);
        Signal.Disconnect<SignalDataNoParamA>(MethodNoParams);
        count = Signal.GetCount<SignalDataNoParamA>();
        Assert.AreEqual(1, count);
    }

    [Test]
    public void SignalSendNoParamTest()
    {
        Signal.Connect<SignalDataNoParamA>(() => { });
        Signal.Connect<SignalDataNoParamA>(MethodNoParams);
        Signal.Send <SignalDataNoParamA> ();
        Assert.AreEqual(1, m_callNoParams);
    }

    [Test]
    public void SignalSendParamTest()
    {
        Signal.Connect<SignalDataParamA>(MethodParamsA);
        string expected = "Test data";
        Signal.Send(new SignalDataParamA(expected));
        Assert.AreEqual(expected, m_data);
    }

    void MethodParamsA(SignalDataParamA data) => m_data = data.data;
    void MethodParamsB(SignalDataParamA data) => m_data = data.data; 
    void MethodNoParams() => m_callNoParams++; 
    public class SignalDataParamA: SignalData 
    {
        public string data;
        public SignalDataParamA(string data)
        {
            this.data = data;  
        }
    }
    public class SignalDataNoParamA : SignalData { }
    public class SignalDataNoParamB : SignalData { }
}
