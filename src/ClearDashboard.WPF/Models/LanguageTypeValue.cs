using ClearDashboard.Wpf.Helpers;
using System;
using System.ComponentModel;

namespace ClearDashboard.Wpf.Models
{
    [TypeConverter(typeof(EnumDefaultValueTypeConverter))]
    public enum LanguageTypeValue
    {
        [RTL(false)]
        [Description("am"), DefaultValue("አማርኛ")]
        am,
        [RTL(true)]
        [Description("ar"), DefaultValue("عربي")]
        ar,
        [RTL(false)]
        [Description("de"), DefaultValue("Deutsche Sprache")]
        de,
        [RTL(false)]
        [Description("en-US"), DefaultValue("English Language")]
        en,
        [RTL(false)]
        [Description("es"), DefaultValue("Lengua española")]
        es,
        [RTL(false)]
        [Description("fr"), DefaultValue("Langue française")]
        fr,
        [RTL(false)]
        [Description("hi"), DefaultValue("हिन्दी")]
        hi,
        [RTL(false)]
        [Description("id"), DefaultValue("Bahasa Indo")]
        id,
        [RTL(false)]
        [Description("km"), DefaultValue("ខ្មែរ")]
        km,
        [RTL(false)]
        [Description("pt"), DefaultValue("Idioma portugues")]
        pt,
        [RTL(false)]
        [Description("pt-BR"), DefaultValue("Língua portuguesa (Brasil)")]
        ptBR,
        [RTL(false)]
        [Description("ro"), DefaultValue("Română")]
        ro,
        [RTL(false)]
        [Description("ru-RU"), DefaultValue("русский")]
        ruRU,
        [RTL(false)]
        [Description("vi"), DefaultValue("Ngôn ngữ tiếng Việt")]
        vi,
        [RTL(false)]
        [Description("zh-CN"), DefaultValue("简体中文")]
        zhCN,
        [RTL(false)]
        [Description("zh-TW"), DefaultValue("繁體中文")]
        zhTW,

    }

    public class RTLAttribute : Attribute
    {
        public bool isRTL;

        public RTLAttribute(bool isRtl)
        {
            isRTL = isRtl;
        }
    }
}
