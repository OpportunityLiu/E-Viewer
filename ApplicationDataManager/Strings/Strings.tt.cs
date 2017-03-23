namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IResourceProvider
    {
        global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider this[string resourceKey] { get; }
        string GetValue(string resourceKey);
    }

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IGeneratedResourceProvider : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        string Value { get; }
    }

    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    [System.Diagnostics.DebuggerDisplay("\\{{Value}\\}")]
    internal struct GeneratedResourceProvider : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IGeneratedResourceProvider
    {
        internal GeneratedResourceProvider(string key)
        {
            this.key = key;
        }

        private readonly string key;

        public string Value => global::ApplicationDataManager.Strings.GetValue(key);

        public GeneratedResourceProvider this[string resourceKey]
        {
            get
            {
                if(resourceKey == null)
                    throw new global::System.ArgumentNullException();
                return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider($"{key}/{resourceKey}");
            }
        }

        public string GetValue(string resourceKey)
        {
            if(resourceKey == null)
                return this.Value;
            return global::ApplicationDataManager.Strings.GetValue($"{key}/{resourceKey}");
        }
    }
}

namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IResources : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.IBoolean Boolean { get; }
    }
}

namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IBoolean : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IYesNo YesNo { get; }
        global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IEnabledDisabled EnabledDisabled { get; }
        global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IOnOff OnOff { get; }
        global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.ITrueFalse TrueFalse { get; }
    }
}

namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IYesNo : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>Yes</para>
        /// </summary>
        string True { get; }
        /// <summary>
        /// <para>No</para>
        /// </summary>
        string False { get; }
    }
}

namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IEnabledDisabled : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>Enabled</para>
        /// </summary>
        string True { get; }
        /// <summary>
        /// <para>Disabled</para>
        /// </summary>
        string False { get; }
    }
}

namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface IOnOff : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>On</para>
        /// </summary>
        string True { get; }
        /// <summary>
        /// <para>Off</para>
        /// </summary>
        string False { get; }
    }
}

namespace ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal interface ITrueFalse : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider
    {
        /// <summary>
        /// <para>True</para>
        /// </summary>
        string True { get; }
        /// <summary>
        /// <para>False</para>
        /// </summary>
        string False { get; }
    }
}

namespace ApplicationDataManager
{
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Resource Generator", "2.0.1.0")]
    internal static class Strings
    {
        private static global::System.Collections.Generic.IDictionary<string, string> __cache____W3alsRt;
        private static global::Windows.ApplicationModel.Resources.ResourceLoader __loader____HsCav8l;

        static Strings()
        {
            global::ApplicationDataManager.Strings.__cache____W3alsRt = new global::System.Collections.Generic.Dictionary<string, string>();
            global::ApplicationDataManager.Strings.__loader____HsCav8l = global::Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse();
        }

        public static string GetValue(string resourceKey)
        {
            string value;
            if(global::ApplicationDataManager.Strings.__cache____W3alsRt.TryGetValue(resourceKey, out value))
                return value;
            return global::ApplicationDataManager.Strings.__cache____W3alsRt[resourceKey] = global::ApplicationDataManager.Strings.__loader____HsCav8l.GetString(resourceKey);
        }


        internal static global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResources Resources { get; } = new global::ApplicationDataManager.Strings.Resources__K2bADuB();

        [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\\}")]
        private sealed class Resources__K2bADuB : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResources
        {
            global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.this[string resourceKey]
            {
                get
                {
                    if(resourceKey == null)
                        throw new global::System.ArgumentNullException();
                    return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources/" + resourceKey);
                }
            }

            string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
            {
                return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources/" + resourceKey);
            }


            public global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.IBoolean Boolean { get; } = new global::ApplicationDataManager.Strings.Resources__K2bADuB.Boolean__Jdx8pdQn();

            [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\\}")]
            private sealed class Boolean__Jdx8pdQn : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.IBoolean
            {
                global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.this[string resourceKey]
                {
                    get
                    {
                        if(resourceKey == null)
                            throw new global::System.ArgumentNullException();
                        return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean/" + resourceKey);
                    }
                }

                string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
                {
                    if(resourceKey == null)
                        return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean");
                    return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean/" + resourceKey);
                }

                public global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IYesNo YesNo { get; } = new global::ApplicationDataManager.Strings.Resources__K2bADuB.Boolean__Jdx8pdQn.YesNo__pAyS0wti();

                [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FYesNo\\}")]
                private sealed class YesNo__pAyS0wti : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IYesNo
                {
                    global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.this[string resourceKey]
                    {
                        get
                        {
                            if(resourceKey == null)
                                throw new global::System.ArgumentNullException();
                            return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FYesNo/" + resourceKey);
                        }
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
                    {
                        if(resourceKey == null)
                            return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FYesNo");
                        return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FYesNo/" + resourceKey);
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IYesNo.True
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FYesNo\u002FTrue");
                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IYesNo.False
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FYesNo\u002FFalse");
                }

                public global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IEnabledDisabled EnabledDisabled { get; } = new global::ApplicationDataManager.Strings.Resources__K2bADuB.Boolean__Jdx8pdQn.EnabledDisabled__iDMN8gBT();

                [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FEnabledDisabled\\}")]
                private sealed class EnabledDisabled__iDMN8gBT : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IEnabledDisabled
                {
                    global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.this[string resourceKey]
                    {
                        get
                        {
                            if(resourceKey == null)
                                throw new global::System.ArgumentNullException();
                            return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FEnabledDisabled/" + resourceKey);
                        }
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
                    {
                        if(resourceKey == null)
                            return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FEnabledDisabled");
                        return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FEnabledDisabled/" + resourceKey);
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IEnabledDisabled.True
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FEnabledDisabled\u002FTrue");
                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IEnabledDisabled.False
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FEnabledDisabled\u002FFalse");
                }

                public global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IOnOff OnOff { get; } = new global::ApplicationDataManager.Strings.Resources__K2bADuB.Boolean__Jdx8pdQn.OnOff__Ug5yJlO();

                [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FOnOff\\}")]
                private sealed class OnOff__Ug5yJlO : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IOnOff
                {
                    global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.this[string resourceKey]
                    {
                        get
                        {
                            if(resourceKey == null)
                                throw new global::System.ArgumentNullException();
                            return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FOnOff/" + resourceKey);
                        }
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
                    {
                        if(resourceKey == null)
                            return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FOnOff");
                        return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FOnOff/" + resourceKey);
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IOnOff.True
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FOnOff\u002FTrue");
                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.IOnOff.False
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FOnOff\u002FFalse");
                }

                public global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.ITrueFalse TrueFalse { get; } = new global::ApplicationDataManager.Strings.Resources__K2bADuB.Boolean__Jdx8pdQn.TrueFalse__GrEMPZW();

                [System.Diagnostics.DebuggerDisplay("\\{ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FTrueFalse\\}")]
                private sealed class TrueFalse__GrEMPZW : global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.ITrueFalse
                {
                    global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.this[string resourceKey]
                    {
                        get
                        {
                            if(resourceKey == null)
                                throw new global::System.ArgumentNullException();
                            return new global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.GeneratedResourceProvider("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FTrueFalse/" + resourceKey);
                        }
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.IResourceProvider.GetValue(string resourceKey)
                    {
                        if(resourceKey == null)
                            return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FTrueFalse");
                        return global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FTrueFalse/" + resourceKey);
                    }

                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.ITrueFalse.True
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FTrueFalse\u002FTrue");
                    string global::ApplicationDataManager.ApplicationDataManager_ResourceInfo.Resources.Boolean.ITrueFalse.False
                        => global::ApplicationDataManager.Strings.GetValue("ms-resource\u003A\u002F\u002F\u002FApplicationDataManager\u002FResources\u002FBoolean\u002FTrueFalse\u002FFalse");
                }

            }

        }
    }
}
