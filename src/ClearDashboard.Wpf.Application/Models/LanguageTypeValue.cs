using System;
using System.ComponentModel;
using ClearDashboard.Wpf.Application.Helpers;

namespace ClearDashboard.Wpf.Application.Models
{
    [TypeConverter(typeof(EnumDefaultValueTypeConverter))]
    public enum LanguageTypeValue
    {
        [RTL(false)]
        [Description("en-US"), DefaultValue("English Language")]//multiple
        enUS,
        [RTL(false)]
        [Description("am-ET"), DefaultValue("አማርኛ")]
        amET,
        [RTL(true)]
        [Description("ar-EG"), DefaultValue("عربي")]//multiple
        arEG,
        [RTL(false)]
        [Description("de-DE"), DefaultValue("Deutsche Sprache")]//multiple
        deDE,
        [RTL(false)]
        [Description("es-MX"), DefaultValue("Lengua española")]//multiple
        esMX,
        [RTL(false)]
        [Description("fr-FR"), DefaultValue("Langue française")]//multiple
        frFR,
        [RTL(false)]
        [Description("hi-IN"), DefaultValue("हिन्दी")]
        hiIN,
        [RTL(false)]
        [Description("id-ID"), DefaultValue("Bahasa Indo")]
        idID,
        [RTL(false)]
        [Description("km-KM"), DefaultValue("ខ្មែរ")]
        kmKM,
        [RTL(false)]
        [Description("pt-PT"), DefaultValue("Idioma portugues")]
        ptPT,
        [RTL(false)]
        [Description("pt-BR"), DefaultValue("Língua portuguesa (Brasil)")]
        ptBR,
        [RTL(false)]
        [Description("ro-RO"), DefaultValue("Română")]//multiple
        roRO,
        [RTL(false)]
        [Description("ru-RU"), DefaultValue("русский")]//multiple
        ruRU,
        [RTL(false)]
        [Description("vi-VN"), DefaultValue("Ngôn ngữ tiếng Việt")]
        viVN,
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
