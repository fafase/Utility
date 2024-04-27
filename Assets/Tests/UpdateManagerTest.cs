using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using UnityEngine;
using UnityEngine.TestTools;
using Tools;
using System.Reflection;
using System;
public class UpdateManagerTest
{
    UpdateManager m_updateManager;
    IUpdateBehaviour m_updateA, m_updateB, m_updateC;
    System.Action m_onUpdate;

    [OneTimeSetUp]
    public void OneTimeSetUp() 
    {
        m_updateA = Substitute.For<IUpdateBehaviour>();
        m_updateB = Substitute.For<IUpdateBehaviour>();
        m_updateC = Substitute.For<IUpdateBehaviour>();

        GameObject obj = new GameObject();
        m_updateManager = obj.AddComponent<UpdateManager>();
        MethodInfo mi = m_updateManager.GetType().GetMethod("Update", BindingFlags.NonPublic | BindingFlags.Instance);
        if(mi != null) 
        {
            m_onUpdate = Delegate.CreateDelegate(typeof(Action), m_updateManager, mi) as Action;
        }
    }
    [SetUp]
    public void SetUp() 
    {
        m_updateManager.UnregisterUpdate(m_updateA);
        m_updateManager.UnregisterUpdate(m_updateB);
        m_updateManager.UnregisterUpdate(m_updateC);
        m_updateA.IsActive.Returns(true);
        m_updateB.IsActive.Returns(true);
        m_updateC.IsActive.Returns(true);
        ClearCalls();
    }

    [Test]
    public void UpdateManagerTestRegistrationPass() 
    { 
        m_updateManager.RegisterUpdate(m_updateA);
        m_onUpdate.Invoke();

        m_updateA.Received().OnUpdate();
        m_updateB.DidNotReceive().OnUpdate();
        m_updateC.DidNotReceive().OnUpdate();
        Assert.AreEqual(1, m_updateManager.UpdateCount);
    }

    [Test]
    public void UpdateManagerTestUnregistrationPass() 
    {
        m_updateManager.RegisterUpdate(m_updateA);
        m_updateManager.RegisterUpdate(m_updateB);
        m_updateManager.RegisterUpdate(m_updateC);
        m_onUpdate.Invoke();

        ClearCalls();

        Assert.AreEqual(3, m_updateManager.UpdateCount);

        m_updateManager.UnregisterUpdate(m_updateA);
        m_updateManager.UnregisterUpdate(m_updateB);
        m_updateManager.UnregisterUpdate(m_updateC);

        m_onUpdate.Invoke();
        m_updateA.DidNotReceive().OnUpdate();
        m_updateB.DidNotReceive().OnUpdate();
        m_updateC.DidNotReceive().OnUpdate();

        Assert.AreEqual(0, m_updateManager.UpdateCount);
    }

    [Test]
    public void UpdateManagerTestUpdatePass() 
    {
        m_updateManager.RegisterUpdate(m_updateA);
        m_updateManager.RegisterUpdate(m_updateB);
        m_updateManager.RegisterUpdate(m_updateC);
        m_onUpdate.Invoke();
        m_updateA.Received().OnUpdate();
        m_updateB.Received().OnUpdate();
        m_updateB.Received().OnUpdate();
    }

    [Test]
    public void UpdateManagerTestUpdateInactivePass() 
    {
        m_updateManager.RegisterUpdate(m_updateA);
        m_updateManager.RegisterUpdate(m_updateB);
        m_updateManager.RegisterUpdate(m_updateC);
        m_updateC.IsActive.Returns(false);

        m_onUpdate.Invoke();

        m_updateA.Received().OnUpdate();
        m_updateB.Received().OnUpdate();
        m_updateC.DidNotReceive().OnUpdate();
        Assert.AreEqual(3, m_updateManager.UpdateCount);
    }

    [Test]
    public void UpdateManagerTestRegisterNullPass() 
    {
        m_updateManager.RegisterUpdate(m_updateA);
        m_updateManager.RegisterUpdate(m_updateB);
        m_updateManager.RegisterUpdate(m_updateC);
        IUpdateBehaviour updateBehaviour = null;    
        m_updateManager.RegisterUpdate(updateBehaviour);
        Assert.AreEqual(3, m_updateManager.UpdateCount);

        m_onUpdate.Invoke();
        m_updateA.Received().OnUpdate();
        m_updateB.Received().OnUpdate();
        m_updateC.Received().OnUpdate();
        Assert.AreEqual(3, m_updateManager.UpdateCount);
    }

    private void ClearCalls() 
    {
        m_updateA.ClearReceivedCalls();
        m_updateB.ClearReceivedCalls();
        m_updateC.ClearReceivedCalls();
    }
}
