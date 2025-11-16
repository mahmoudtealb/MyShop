using System.Globalization;

namespace MyShop.Utilities
{
    public static class CurrencyHelper
    {
        // استخدام الإعدادات الموحدة
        private static string CURRENCY_SYMBOL => CurrencySettings.SYMBOL;
        private static string CURRENCY_CODE => CurrencySettings.CODE;
        private static string CURRENCY_POSITION => CurrencySettings.POSITION;
        
        /// <summary>
        /// تنسيق العملة بشكل موحد
        /// </summary>
        /// <param name="amount">المبلغ</param>
        /// <returns>المبلغ المنسق</returns>
        public static string FormatCurrency(decimal amount)
        {
            var formattedAmount = amount.ToString($"N{CurrencySettings.DECIMAL_PLACES}", CultureInfo.InvariantCulture);
            
            if (CurrencySettings.SHOW_SYMBOL)
            {
                if (CURRENCY_POSITION == "BEFORE")
                    return $"{CURRENCY_SYMBOL}{formattedAmount}";
                else
                    return $"{formattedAmount} {CURRENCY_SYMBOL}";
            }
            
            return formattedAmount;
        }
        
        /// <summary>
        /// تنسيق العملة مع رمز مخصص
        /// </summary>
        /// <param name="amount">المبلغ</param>
        /// <param name="symbol">رمز العملة</param>
        /// <returns>المبلغ المنسق</returns>
        public static string FormatCurrency(decimal amount, string symbol)
        {
            // تنسيق الرقم مع فاصلة عشرية
            var formattedAmount = amount.ToString("N2", CultureInfo.InvariantCulture);
            
            // إضافة رمز العملة
            return $"{symbol}{formattedAmount}";
        }
        
        /// <summary>
        /// تنسيق العملة مع رمز العملة في النهاية
        /// </summary>
        /// <param name="amount">المبلغ</param>
        /// <returns>المبلغ المنسق</returns>
        public static string FormatCurrencySuffix(decimal amount)
        {
            var formattedAmount = amount.ToString("N2", CultureInfo.InvariantCulture);
            return $"{formattedAmount} {CURRENCY_SYMBOL}";
        }
        
        /// <summary>
        /// تنسيق العملة مع رمز العملة في البداية (عربي)
        /// </summary>
        /// <param name="amount">المبلغ</param>
        /// <returns>المبلغ المنسق</returns>
        public static string FormatCurrencyArabic(decimal amount)
        {
            var formattedAmount = amount.ToString("N2", CultureInfo.InvariantCulture);
            return $"{CURRENCY_SYMBOL} {formattedAmount}";
        }
        
        /// <summary>
        /// تنسيق العملة مع تنسيق جميل
        /// </summary>
        /// <param name="amount">المبلغ</param>
        /// <returns>المبلغ المنسق مع تنسيق جميل</returns>
        public static string FormatCurrencyPretty(decimal amount)
        {
            var formattedAmount = amount.ToString("N2", CultureInfo.InvariantCulture);
            return $"<span class='currency-symbol'>{CURRENCY_SYMBOL}</span><span class='currency-amount'>{formattedAmount}</span>";
        }
        
        /// <summary>
        /// تنسيق العملة مع فئات مختلفة
        /// </summary>
        /// <param name="amount">المبلغ</param>
        /// <param name="isLarge">هل المبلغ كبير (يحتاج تنسيق خاص)</param>
        /// <returns>المبلغ المنسق</returns>
        public static string FormatCurrency(decimal amount, bool isLarge)
        {
            if (isLarge && amount >= 1000000)
            {
                var millions = amount / 1000000;
                return $"{CURRENCY_SYMBOL}{millions:F1}M";
            }
            else if (isLarge && amount >= 1000)
            {
                var thousands = amount / 1000;
                return $"{CURRENCY_SYMBOL}{thousands:F1}K";
            }
            else
            {
                return FormatCurrency(amount);
            }
        }
    }
}
