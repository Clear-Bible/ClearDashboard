using System;
using System.ComponentModel;
using ClearDashboard.Wpf.Application.Helpers;

namespace ClearDashboard.Wpf.Application.Models
{
    [TypeConverter(typeof(EnumDefaultValueTypeConverter))]
    public enum LanguageTypeValue
    {
        [RTL(false)]
        [Description("en"), DefaultValue("English Language")]//multiple
        en,
        [RTL(false)]
        [Description("am"), DefaultValue("አማርኛ")]
        am,
        [RTL(true)]
        [Description("ar"), DefaultValue("عربي")]//multiple
        ar,
        [RTL(false)]
        [Description("de"), DefaultValue("Deutsche Sprache")]//multiple
        de,
        [RTL(false)]
        [Description("es"), DefaultValue("Lengua española")]//multiple
        es,
        [RTL(false)]
        [Description("fr"), DefaultValue("Langue française")]//multiple
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
        [Description("pt-PT"), DefaultValue("Idioma portugues")]
        ptPT,
        [RTL(false)]
        [Description("pt-BR"), DefaultValue("Língua portuguesa (Brasil)")]
        ptBR,
        [RTL(false)]
        [Description("ro"), DefaultValue("Română")]//multiple
        ro,
        [RTL(false)]
        [Description("ru"), DefaultValue("Русский")]//multiple
        ru,
        [RTL(false)]
        [Description("vi"), DefaultValue("Ngôn ngữ tiếng Việt")]
        vi,
        [RTL(false)]
        [Description("zh-CN"), DefaultValue("简体中文")]//multiple
        zhCN,
        [RTL(false)]
        [Description("zh-TW"), DefaultValue("繁體中文")]//multiple
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
