using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using Tools;
using UnityEditor;
using UnityEngine;

public class UserPrefsTest
{
    private IUserPrefs m_userPrefs;
    private DateTime m_date = new DateTime(2020, 10, 1, 0, 0, 0 );
    [OneTimeSetUp]
    public void OneTimeSetUp() 
    {
        m_userPrefs = new UserPrefs();
        m_userPrefs.Init();
    }

    [SetUp]
    public void SetUp() 
    {
        m_userPrefs.ClearUserPrefs();
    }

    [Test]
    public void UserPrefsTestInitPass()
    {
        string json = m_userPrefs.Json;
        Assert.AreEqual("{}", json);
    }

    [Test]
    public void UserPrefsTestMissingIntEmptyPass() 
    {
        bool result = m_userPrefs.TryGetInt("MissingKey", out int value, 10);
        Assert.IsTrue(!result);  
        Assert.AreEqual(value, 10);
    }

    [Test]
    public void UserPrefsTestMissingBoolEmptyPass()
    {
        bool result = m_userPrefs.TryGetBool("MissingKey", out bool value);
        Assert.IsTrue(!result);
        Assert.IsFalse(value);
    }

    [Test]
    public void UserPrefsTestMissingFloatEmptyPass()
    {
        bool result = m_userPrefs.TryGetFloat("MissingKey", out float value, 100.0f);
        Assert.IsTrue(!result);
        Assert.AreEqual(100.0f, value);
    }

    [Test]
    public void UserPrefsTestMissingDateTimeEmptyPass()
    {
        bool result = m_userPrefs.TryGetDate("MissingKey", out DateTime value, m_date);
        Assert.IsTrue(!result);
        Assert.AreEqual(2020, value.Year);
        Assert.AreEqual(10, value.Month);
        Assert.AreEqual(1, value.Day);
    }

    [Test]
    public void UserPrefsTestMissingObjectEmptyPass()
    {
        bool result = m_userPrefs.TryGetObject<TestClass>("MissingObject", out TestClass value, null);
        Assert.IsTrue(!result);
        Assert.IsNull(value);
    }

    [Test]
    public void UserPrefsTestMissingIntPass()
    {
        m_userPrefs.SetValue("KeyA", 20);
        m_userPrefs.SetValue("KeyB", new TestClass(10, 20.0f));
        bool result = m_userPrefs.TryGetInt("MissingKey", out int value, 10);
        Assert.IsTrue(!result);
        Assert.AreEqual(value, 10);
    }

    [Test]
    public void UserPrefsTestMissingFloatPass()
    {
        m_userPrefs.SetValue("KeyA", 20);
        m_userPrefs.SetValue("KeyB", new TestClass(10, 20.0f));
        bool result = m_userPrefs.TryGetFloat("MissingKey", out float value, 100.0f);
        Assert.IsTrue(!result);
        Assert.AreEqual(100.0f, value);
    }

    [Test]
    public void UserPrefsTestMissingDateTimePass()
    {
        m_userPrefs.SetValue("KeyA", 20);
        m_userPrefs.SetValue("KeyB", new TestClass(10, 20.0f));
        bool result = m_userPrefs.TryGetDate("MissingKey", out DateTime value, m_date);
        Assert.IsTrue(!result);
        Assert.AreEqual(2020, value.Year);
        Assert.AreEqual(10, value.Month);
        Assert.AreEqual(1, value.Day);
    }

    [Test]
    public void UserPrefsTestMissingObjectPass()
    {
        m_userPrefs.SetValue("KeyA", 20);
        m_userPrefs.SetValue("KeyB", new TestClass(10, 20.0f));
        bool result = m_userPrefs.TryGetObject<TestClass>("MissingObject", out TestClass value, null);
        Assert.IsTrue(!result);
        Assert.IsNull(value);
    }

    [Test]
    public void UserPrefsTestRemoveMissingValuePass()
    {
        bool result = m_userPrefs.RemoveKey("MissingKey");
        Assert.IsFalse(result);
    }

    [Test]
    public void UserPrefsTestRemoveValuePass()
    {
        string key = "Key";
        m_userPrefs.SetValue(key, 10);
        bool resA = m_userPrefs.TryGetInt(key, out int value, 0);
        bool resB = m_userPrefs.RemoveKey(key);
        bool resC = m_userPrefs.TryGetInt(key, out int removed, -1);
        Assert.IsTrue(resA && resB && !resC);
        Assert.AreEqual(10, value);
        Assert.AreEqual(-1, removed);
    }

    [Test]
    public void UserPrefsTestRemoveArrayPass()
    {
        string keyPlayer = "player"; string keyScore = "scores"; string keyLogin = "lastLogin";
        JObject initial = new JObject();
        initial[keyPlayer] = "Name"; initial[keyScore] = JArray.FromObject(new int[] { 0, 5, 10 }); initial[keyLogin] = new DateTime(2015, 12, 25, 11, 30, 00);
        m_userPrefs.SetUserPrefsFromRemote(initial.ToString());

        bool resA = m_userPrefs.TryGetObject(keyScore, out int[] valuesA);

        m_userPrefs.RemoveKey(keyScore);
        bool resB = m_userPrefs.TryGetObject(keyScore, out int[] valuesB);
        Assert.IsTrue(resA && !resB);
        Assert.AreEqual(3, valuesA.Length);
        Assert.IsNull(valuesB);
    }

    [Test]
    public void UserPrefsTestAddIntPass() 
    {
        string key = "Key";
        m_userPrefs.SetValue(key, 10);
        bool resultA = m_userPrefs.TryGetInt(key, out int value, 0);
        Assert.AreEqual(10, value);
        m_userPrefs.SetValue(key, 20);
        bool resultB = m_userPrefs.TryGetInt(key, out value, 0);
        Assert.IsTrue(resultA && resultB);
        Assert.AreEqual(20, value);
    }

    [Test]
    public void UserPrefsTestAddBoolPass()
    {
        string key = "Key";
        m_userPrefs.SetValue(key, true);
        bool resultA = m_userPrefs.TryGetBool(key, out bool value);
        Assert.IsTrue(value);
        m_userPrefs.SetValue(key, false);
        bool resultB = m_userPrefs.TryGetBool(key, out value);
        Assert.IsTrue(resultA && resultB);
        Assert.IsFalse(value);
    }

    [Test]
    public void UserPrefsTestAddFloatPass()
    {
        string key = "Key";
        m_userPrefs.SetValue(key, 10.0f);
        bool resultA = m_userPrefs.TryGetFloat(key, out float value, 0.0f);
        Assert.AreEqual(10.0f, value);
        m_userPrefs.SetValue(key, 20.0f);
        bool resultB = m_userPrefs.TryGetFloat(key, out value, 0.0f);
        Assert.IsTrue(resultA && resultB);
        Assert.AreEqual(20.0f, value);
    }

    [Test]
    public void UserPrefsTestAddDateTimePass()
    {
        string key = "Key";
        m_userPrefs.SetValue(key, m_date);
        bool resultA = m_userPrefs.TryGetDate(key, out DateTime value, new DateTime(1970,1,1,0,0,0));
        Assert.AreEqual(m_date, value);

        DateTime d = m_date.AddDays(1);
        m_userPrefs.SetValue(key, d);
        bool resultB = m_userPrefs.TryGetDate(key, out value, new DateTime(1970, 1, 1, 0, 0, 0));
        Assert.IsTrue(resultA && resultB);
        Assert.AreEqual(2020, value.Year);
        Assert.AreEqual(10, value.Month);
        Assert.AreEqual(2, value.Day);
    }

    [Test]
    public void UserPrefsTestAddObjectPass()
    {
        string key = "newObj";
        TestClass obj = new TestClass(10, 20.0f);
        m_userPrefs.SetValue(key, obj);

        bool result = m_userPrefs.TryGetObject(key, out TestClass value, new TestClass(0, 0.0f));
        Assert.IsTrue(result);
        Assert.AreEqual(10, value.a);
        Assert.AreEqual(20.0f, value.b);
    }

    [Test]
    public void UserPrefsTestAddListPass()
    {
        string key = "newObj";
        List<int> values = new List<int>() 
        {
            0,5,10,15
        };
        m_userPrefs.SetValue(key, values);

        bool result = m_userPrefs.TryGetObject(key, out List<int> value, null);
        Assert.IsTrue(result);
        Assert.AreEqual(0, value[0]);
        Assert.AreEqual(4, value.Count);
    }

    [Test]
    public void UserPrefsTestAddArrayPass()
    {
        string key = "newObj";
        int[] values = new int[]
        {
            0,5,10,15
        };
        m_userPrefs.SetValue(key, values);

        bool result = m_userPrefs.TryGetObject(key, out int[] value, null);
        Assert.IsTrue(result);
        Assert.AreEqual(0, value[0]);
        Assert.AreEqual(4, value.Length);
    }

    [Test]
    public void UserPrefsTestAddArrayGetListPass()
    {
        string key = "newObj";
        int[] values = new int[]
        {
            0,5,10,15
        };
        m_userPrefs.SetValue(key, values);

        bool result = m_userPrefs.TryGetObject(key, out List<int> value, null);
        Assert.IsTrue(result);
        Assert.AreEqual(0, value[0]);
        Assert.AreEqual(4, value.Count);
    }

    [Test]
    public void UserPrefsTestAddListGetArrayPass()
    {
        string key = "newObj";
        List<int> values = new List<int>
        {
            0,5,10,15
        };
        m_userPrefs.SetValue(key, values);

        bool result = m_userPrefs.TryGetObject(key, out int[] value, null);
        Assert.IsTrue(result);
        Assert.AreEqual(0, value[0]);
        Assert.AreEqual(4, value.Length);
    }

    [Test]
    public void UserPrefsTestAddMultiEntryPass()
    {
        string keyA = "KeyA"; string keyB = "KeyB";string keyC = "KeyC";
        TestClass testClass = new TestClass(20, 3.5f);
        
        m_userPrefs.SetValue(keyA, 10);
        m_userPrefs.SetValue(keyB, testClass);
        m_userPrefs.SetValue(keyC, "Lucas");
        bool resA = m_userPrefs.TryGetInt(keyA, out int intVal, 0);
        bool resB = m_userPrefs.TryGetObject(keyB, out TestClass testVal, null);
        bool resC = m_userPrefs.TryGetObject(keyC, out string name, null);
        Assert.IsTrue(resA && resB && resC);
        Assert.AreEqual(10, intVal);
        Assert.AreEqual(20, testVal.a);
        Assert.AreEqual(3.5f, testVal.b);
        Assert.AreEqual("Lucas", name);
    }

    [Test]
    public void UserPrefsTestRemotePass() 
    {
        string keyPlayer = "player";string keyScore = "score"; string keyLogin = "lastLogin";
        JObject json = new JObject();
        json[keyPlayer] = "Name"; json[keyScore] = 1000; json[keyLogin] = new DateTime(2015, 12, 25, 11, 30, 00);
        string remoteJson = json.ToString();

        m_userPrefs.SetUserPrefsFromRemote(remoteJson);
        bool resA = m_userPrefs.TryGetObject("player", out string player, "");
        bool resB = m_userPrefs.TryGetInt("score", out int score, 0);
        bool resC =  m_userPrefs.TryGetDate("lastLogin", out DateTime date, DateTime.Now);
        Assert.IsTrue(resA && resB && resC);
        Assert.AreEqual("Name", player);
        Assert.AreEqual(1000, score);
        Assert.AreEqual("12/25/2015 11:30:00 AM", date.ToString());
    }
 
    [Test]
    public void UserPrefsMergePass() 
    {
        string keyPlayer = "player"; string keyScore = "scores"; string keyLogin = "lastLogin";
        JObject initial = new JObject();
        initial[keyPlayer] = "Name"; initial[keyScore] = JArray.FromObject( new int[] { 0, 5, 10 }); initial[keyLogin] = new DateTime(2015, 12, 25, 11, 30, 00);
        m_userPrefs.SetUserPrefsFromRemote(initial.ToString());
        bool resA = m_userPrefs.TryGetObject(keyScore, out int[] valuesA);

        string keyEnabled = "enabled"; 
        JObject merge = new JObject();
        merge[keyEnabled] = true; merge[keyScore] = JArray.FromObject(new int[] { 0, 10, 15 });
        m_userPrefs.MergeContentFromRemote(merge.ToString());

        bool resB = m_userPrefs.TryGetObject(keyScore, out int[] valuesB);
        Assert.IsTrue(resA && resB);
        Assert.AreEqual(3, valuesA.Length);
        Assert.AreEqual(4, valuesB.Length);
    }

    [Serializable]
    public class TestClass
    {
        public int a;
        public float b;
        public TestClass(int a, float b)
        {
            this.a = a;
            this.b = b;
        }
    }
}
