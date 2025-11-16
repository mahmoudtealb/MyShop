# إعدادات العملة الموحدة

## نظرة عامة
تم إنشاء نظام موحد للعملة في التطبيق لضمان عرض العملة بشكل متسق في جميع أنحاء التطبيق.

## كيفية تغيير العملة

### 1. تغيير العملة الأساسية
افتح ملف `MyShop.Utilities/CurrencySettings.cs` وغير الإعدادات التالية:

```csharp
public static class CurrencySettings
{
    // غير هذه القيم لتغيير العملة
    public const string SYMBOL = "$";           // رمز العملة
    public const string CODE = "USD";           // كود العملة
    public const string ARABIC_NAME = "دولار أمريكي";  // الاسم العربي
    public const string ENGLISH_NAME = "US Dollar";    // الاسم الإنجليزي
}
```

### 2. العملات المتاحة

#### الدولار الأمريكي (USD)
```csharp
public const string SYMBOL = "$";
public const string CODE = "USD";
public const string ARABIC_NAME = "دولار أمريكي";
```

#### الجنيه المصري (EGP)
```csharp
public const string SYMBOL = "ج.م";
public const string CODE = "EGP";
public const string ARABIC_NAME = "جنيه مصري";
```

#### اليورو (EUR)
```csharp
public const string SYMBOL = "€";
public const string CODE = "EUR";
public const string ARABIC_NAME = "يورو";
```

#### الجنيه الإسترليني (GBP)
```csharp
public const string SYMBOL = "£";
public const string CODE = "GBP";
public const string ARABIC_NAME = "جنيه إسترليني";
```

### 3. إعدادات التنسيق

```csharp
public const string POSITION = "BEFORE";        // موضع رمز العملة
public const int DECIMAL_PLACES = 2;            // عدد الأرقام العشرية
public const bool SHOW_SYMBOL = true;           // عرض رمز العملة
public const bool USE_PRETTY_FORMAT = true;     // استخدام تنسيق جميل
```

### 4. كيفية الاستخدام

#### في الـ Views:
```html
@using MyShop.Utilities

<!-- تنسيق بسيط -->
@CurrencyHelper.FormatCurrency(Model.Price)

<!-- تنسيق مع رمز مخصص -->
@CurrencyHelper.FormatCurrency(Model.Price, "€")

<!-- تنسيق جميل -->
@Html.Raw(CurrencyHelper.FormatCurrencyPretty(Model.Price))
```

#### في الـ Controllers:
```csharp
using MyShop.Utilities;

// تنسيق العملة
var formattedPrice = CurrencyHelper.FormatCurrency(product.Price);
ViewBag.FormattedPrice = formattedPrice;
```

### 5. الملفات المتأثرة

عند تغيير العملة، سيتم تحديث العرض في:
- ✅ Dashboard (لوحة التحكم)
- ✅ Order Details (تفاصيل الطلب)
- ✅ Product Views (صفحات المنتجات)
- ✅ Cart Views (صفحات السلة)
- ✅ جميع الصفحات التي تستخدم `ToString("C")`

### 6. نصائح مهمة

1. **تأكد من التطبيق**: بعد تغيير العملة، تأكد من إعادة تشغيل التطبيق
2. **اختبار شامل**: اختبر جميع الصفحات للتأكد من عرض العملة بشكل صحيح
3. **قاعدة البيانات**: تأكد من أن البيانات في قاعدة البيانات متوافقة مع العملة الجديدة
4. **Stripe**: إذا كنت تستخدم Stripe، تأكد من تحديث إعدادات العملة هناك أيضاً

### 7. مثال كامل لتغيير العملة إلى الجنيه المصري

```csharp
// في CurrencySettings.cs
public static class CurrencySettings
{
    public const string SYMBOL = "ج.م";
    public const string CODE = "EGP";
    public const string ARABIC_NAME = "جنيه مصري";
    public const string ENGLISH_NAME = "Egyptian Pound";
    public const string POSITION = "AFTER";  // رمز العملة بعد الرقم
}
```

### 8. استكشاف الأخطاء

إذا لم تظهر العملة الجديدة:
1. تأكد من إعادة تشغيل التطبيق
2. تأكد من استخدام `@using MyShop.Utilities` في الـ Views
3. تأكد من استدعاء `CurrencyHelper.FormatCurrency()` بدلاً من `ToString("C")`
4. تحقق من ملف `Program.cs` للتأكد من إعدادات الثقافة

---

**ملاحظة**: هذا النظام يضمن عرض العملة بشكل موحد في جميع أنحاء التطبيق. أي تغيير في `CurrencySettings.cs` سيؤثر على جميع الصفحات.




