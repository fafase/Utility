using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Tools;
using UnityEditor;
using System;
using Newtonsoft.Json.Linq;
using JetBrains.Annotations;
using NUnit.Framework.Internal;

public class LocalizationTest
{
    private ILocalization m_localization;
    [OneTimeSetUp]
    public void OneTimeSetUp() 
    {
        m_localization = Tools.EditorUtility.LoadScriptableObject<Localization>();
        m_localization.IsInit = true;
    }
    [Test]
    public void LocalizationTestMissingDefaultPass()
    {
        ILocalization localization = ScriptableObject.CreateInstance<Localization>();
        Assert.IsNotNull(localization);
        Exception e = Assert.Throws<Exception>(localization.SetDefault);
        Assert.That(e.Message, Is.EqualTo("[Localization] Missing default localization"));
    }
    
    [Test]
    public void LocalizationTestMissingKeyPass()
    {
        string missing = m_localization.GetLocalization("missingKey");   
        string defaultValue = m_localization.GetLocalization("missingKey", "Default");

        Assert.IsNull(missing);
        Assert.AreEqual("Default", defaultValue);
    }

    [Test]
    public void LocalizationTestGetLocalizationPass() 
    {
        m_localization.SetDefault();
        string name = m_localization.GetLocalization("appName");
        string version = m_localization.GetLocalization("version");
        string description = m_localization.GetLocalization("EVENTS/description");
        string settings = m_localization.GetLocalization("GENERIC/settings");

        Assert.AreEqual("GameName", name);
        Assert.AreEqual("1.0.0", version);
        Assert.AreEqual("Here is the description of the event", description);
        Assert.AreEqual("Settings", settings);
    }


    [Test]
    public void LocalizationTestRemoteLocalizationPass() 
    {
        JObject json = new JObject();
        json["version"] = "1.0.1"; json["locale"] = "en-EN"; json["appName"] = "GameName";
        json["GENERIC"] = new JObject 
        {   { "settings",  "Settings" }
        };
        json["EVENTS"] = new JObject
        {   { "eventName",  "Name for event" },
            { "description", "This event" },
            { "score", "Score" }
        };

        m_localization.SetLocalizationFromRemote(new List<string>() { json.ToString() });
        m_localization.SetWithLocale("en-EN");

        string name = m_localization.GetLocalization("appName");
        string version = m_localization.GetLocalization("version");
        string description = m_localization.GetLocalization("EVENTS/description");
        string score = m_localization.GetLocalization("EVENTS/score");
        string loading = m_localization.GetLocalization("GENERIC/loading");
        
        Assert.AreEqual("GameName", name);
        Assert.AreEqual("1.0.1", version);
        Assert.AreEqual("This event", description);
        Assert.AreEqual("Score", score);
        Assert.IsNull(loading);
    }

    [Test]
    public void LocalizationTestSetDefaultPass()
    {
        m_localization.SetDefault();
        string locale = m_localization.Locale;
        Assert.AreEqual("en-EN", locale);
    }

    [Test]
    public void LocalizationTestSwapLanguagePass() 
    {
        m_localization.SetWithLocale("fr-FR");
        string locale = m_localization.Locale;
        string loading = m_localization.GetLocalization("GENERIC/loading");
          
        Assert.AreEqual("fr-FR", locale);
        Assert.AreEqual("Chargement", loading);
    }

    [Test]
    public void LocalizationTestFormatInputPass()
    {
        m_localization.SetWithLocale("en-EN");

        List<LocArgument> locFormats = new List<LocArgument>()
        { 
            new LocArgument("playerScore", 1555.ToString()),
            new LocArgument("name", "Jeff") 
        };
        string input = m_localization.GetLocalization("EVENTS/score");
        string resultScore = m_localization.FormatLocalizations(input, locFormats);
        locFormats = new List<LocArgument>()
        {
            new LocArgument("test", "This is a test")
        };
        input = m_localization.GetLocalization("EVENTS/otherFormat");
        string resultTest = m_localization.FormatLocalizations(input, locFormats);

        locFormats = new List<LocArgument>()
        {
            new LocArgument("test", "")
        };
        input = m_localization.GetLocalization("EVENTS/otherFormat");
        string resultMissing = m_localization.FormatLocalizations(input, locFormats);

        Assert.AreEqual("Score 1555, well done Jeff!", resultScore);
        Assert.AreEqual("This is a test here", resultTest);
        Assert.AreEqual("{ } here", resultMissing);
    }
    [Test]
    public void LocalizationTestSwapLanguageResultPass()
    {
        m_localization.SetDefault();

        bool resultEn = m_localization.SetWithLocale("en-EN");
        bool resultSu = m_localization.SetWithLocale("su-SU");

        Assert.IsTrue(resultEn);
        Assert.IsFalse(resultSu);
        Assert.AreEqual("en-EN", m_localization.Locale);
    }
}
