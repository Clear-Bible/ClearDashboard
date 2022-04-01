using ClearDashboard.Wpf.Helpers;
using System.ComponentModel;

namespace ClearDashboard.Wpf.Models
{
    [TypeConverter(typeof(EnumDefaultValueTypeConverter))]
    public enum LanguageTypeValue
    {
        [Description("am"), DefaultValue("አማርኛ")]
        am,
        [Description("ar"), DefaultValue("عربي")]
        ar,
        [Description("de"), DefaultValue("Deutsche Sprache")]
        de,
        [Description("en-US"), DefaultValue("English Language")]
        en,
        [Description("es"), DefaultValue("Lengua española")]
        es,
        [Description("fr"), DefaultValue("Langue française")]
        fr,
        [Description("hi"), DefaultValue("हिन्दी")]
        hi,
        [Description("id"), DefaultValue("Bahasa Indo")]
        id,
        [Description("km"), DefaultValue("ខ្មែរ")]
        km,
        [Description("pt"), DefaultValue("Idioma portugues")]
        pt,
        [Description("pt-BR"), DefaultValue("Língua portuguesa (Brasil)")]
        ptBR,
        [Description("ro"), DefaultValue("Română")]
        ro,
        [Description("ru-RU"), DefaultValue("русский")]
        ruRU,
        [Description("vi"), DefaultValue("Ngôn ngữ tiếng Việt")]
        vi,
        [Description("zh-CN"), DefaultValue("简体中文")]
        zhCN,
        [Description("zh-TW"), DefaultValue("繁體中文")]
        zhTW,

    }
}
