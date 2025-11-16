# تطبيق الشريط الجانبي على جميع الصفحات

## التغييرات المنجزة

### 1. إنشاء Layout جديد مع الشريط الجانبي
- تم إنشاء ملف `_LayoutWithSidebar.cshtml` في `MyShop.Web/Views/Shared/`
- يحتوي على الشريط الجانبي من AdminLTE مع تصميم محسن
- يتضمن جميع الروابط المهمة (الرئيسية، السلة، لوحة التحكم، إلخ)

### 2. تحديث ViewStart الملفات
- `MyShop.Web/Views/_ViewStart.cshtml` - تم تغيير Layout إلى `_LayoutWithSidebar`
- `MyShop.Web/Areas/Customer/Views/_ViewStart.cshtml` - تم تغيير Layout إلى `_LayoutWithSidebar`
- `MyShop.Web/Areas/Admin/Views/_ViewStart.cshtml` - تم تغيير Layout إلى `_LayoutWithSidebar`
- `MyShop.Web/Areas/Identity/Pages/_ViewStart.cshtml` - تم تغيير Layout إلى `_LayoutWithSidebar`

### 3. استثناء صفحات الدفع
تم تحديث صفحات الدفع لاستخدام Layout بدون الشريط الجانبي:
- `MyShop.Web/Areas/Customer/Views/Cart/Summary.cshtml` - يستخدم `_Layout`
- `MyShop.Web/Areas/Customer/Views/Cart/OrderConfirmation.cshtml` - يستخدم `_Layout`
- `MyShop.Web/Areas/Customer/Views/Cart/Index.cshtml` - يستخدم `_Layout`

### 4. تحسينات CSS
تم إضافة تحسينات على `MyShop.Web/wwwroot/css/site.css`:
- تأثيرات hover للروابط في الشريط الجانبي
- تدرجات لونية جذابة
- ظلال وتأثيرات بصرية محسنة
- تمييز الصفحة النشطة

### 5. ميزات الشريط الجانبي
- **الرئيسية**: رابط للصفحة الرئيسية
- **سلة التسوق**: رابط لسلة التسوق مع عداد العناصر
- **لوحة التحكم**: للمديرين فقط
- **الفئات**: للمديرين فقط
- **المنتجات**: للمديرين فقط
- **المستخدمين**: للمديرين فقط
- **الملف الشخصي**: للمستخدمين العاديين
- **تسجيل الدخول/التسجيل**: للزوار

### 6. التصميم المتجاوب
- الشريط الجانبي قابل للطي على الشاشات الصغيرة
- زر hamburger menu للتحكم في عرض الشريط الجانبي
- تصميم متجاوب لجميع أحجام الشاشات

## كيفية الاستخدام

1. **تشغيل المشروع**: `dotnet run` في مجلد `MyShop.Web`
2. **الوصول للصفحة الرئيسية**: ستجد الشريط الجانبي على اليسار
3. **التنقل**: استخدم الروابط في الشريط الجانبي للتنقل بين الصفحات
4. **طي الشريط الجانبي**: اضغط على زر hamburger menu في الأعلى

## الملفات المحدثة

### ملفات Layout:
- `MyShop.Web/Views/Shared/_LayoutWithSidebar.cshtml` (جديد)
- `MyShop.Web/Views/Shared/_Layout.cshtml` (محدث)

### ملفات ViewStart:
- `MyShop.Web/Views/_ViewStart.cshtml`
- `MyShop.Web/Areas/Customer/Views/_ViewStart.cshtml`
- `MyShop.Web/Areas/Admin/Views/_ViewStart.cshtml`
- `MyShop.Web/Areas/Identity/Pages/_ViewStart.cshtml`

### صفحات الدفع:
- `MyShop.Web/Areas/Customer/Views/Cart/Summary.cshtml`
- `MyShop.Web/Areas/Customer/Views/Cart/OrderConfirmation.cshtml`
- `MyShop.Web/Areas/Customer/Views/Cart/Index.cshtml`

### ملفات CSS:
- `MyShop.Web/wwwroot/css/site.css`

## ملاحظات مهمة

1. **صفحات الدفع**: تم استثناؤها من الشريط الجانبي لضمان تجربة دفع سلسة
2. **الأمان**: الروابط الإدارية تظهر فقط للمديرين
3. **الأداء**: تم تحسين مسارات الملفات والصور
4. **التوافق**: يعمل مع جميع المتصفحات الحديثة

## استكشاف الأخطاء

إذا واجهت أي مشاكل:
1. تأكد من وجود جميع ملفات AdminLTE في المسارات الصحيحة
2. تحقق من وجود ملفات الصور في `wwwroot/img/`
3. تأكد من تحديث جميع ملفات ViewStart
4. تحقق من عدم وجود أخطاء في console المتصفح 