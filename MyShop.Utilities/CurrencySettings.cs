namespace MyShop.Utilities
{
    /// <summary>
    /// إعدادات العملة الموحدة للتطبيق
    /// يمكن تغيير هذه الإعدادات لتغيير العملة في جميع أنحاء التطبيق
    /// </summary>
    public static class CurrencySettings
    {
        // ===== إعدادات العملة الأساسية =====
        
        /// <summary>
        /// رمز العملة (مثل $ أو ج.م)
        /// </summary>
        public const string SYMBOL = "$";
        
        /// <summary>
        /// كود العملة (مثل USD أو EGP)
        /// </summary>
        public const string CODE = "USD";
        
        /// <summary>
        /// اسم العملة باللغة العربية
        /// </summary>
        public const string ARABIC_NAME = "دولار أمريكي";
        
        /// <summary>
        /// اسم العملة باللغة الإنجليزية
        /// </summary>
        public const string ENGLISH_NAME = "US Dollar";
        
        // ===== إعدادات التنسيق =====
        
        /// <summary>
        /// موضع رمز العملة (BEFORE = قبل الرقم، AFTER = بعد الرقم)
        /// </summary>
        public const string POSITION = "BEFORE";
        
        /// <summary>
        /// عدد الأرقام العشرية
        /// </summary>
        public const int DECIMAL_PLACES = 2;
        
        /// <summary>
        /// رمز الفاصل العشري
        /// </summary>
        public const string DECIMAL_SEPARATOR = ".";
        
        /// <summary>
        /// رمز فاصل الآلاف
        /// </summary>
        public const string THOUSANDS_SEPARATOR = ",";
        
        // ===== إعدادات العرض =====
        
        /// <summary>
        /// هل يتم عرض رمز العملة
        /// </summary>
        public const bool SHOW_SYMBOL = false;
        
        /// <summary>
        /// هل يتم عرض كود العملة
        /// </summary>
        public const bool SHOW_CODE = false;
        
        /// <summary>
        /// هل يتم استخدام تنسيق جميل
        /// </summary>
        public const bool USE_PRETTY_FORMAT = true;
        
        // ===== إعدادات العملات المختلفة =====
        
        /// <summary>
        /// إعدادات الدولار الأمريكي
        /// </summary>
        public static class USD
        {
            public const string SYMBOL = "$";
            public const string CODE = "USD";
            public const string ARABIC_NAME = "دولار أمريكي";
            public const string ENGLISH_NAME = "US Dollar";
        }
        
        /// <summary>
        /// إعدادات الجنيه المصري
        /// </summary>
        public static class EGP
        {
            public const string SYMBOL = "ج.م";
            public const string CODE = "EGP";
            public const string ARABIC_NAME = "جنيه مصري";
            public const string ENGLISH_NAME = "Egyptian Pound";
        }
        
        /// <summary>
        /// إعدادات اليورو
        /// </summary>
        public static class EUR
        {
            public const string SYMBOL = "€";
            public const string CODE = "EUR";
            public const string ARABIC_NAME = "يورو";
            public const string ENGLISH_NAME = "Euro";
        }
        
        /// <summary>
        /// إعدادات الجنيه الإسترليني
        /// </summary>
        public static class GBP
        {
            public const string SYMBOL = "£";
            public const string CODE = "GBP";
            public const string ARABIC_NAME = "جنيه إسترليني";
            public const string ENGLISH_NAME = "British Pound";
        }
    }
}

