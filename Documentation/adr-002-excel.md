# ADR 002: Вибір технології (бібліотеки) для роботи з файлами \*.xlsx для C\#

### Дата: 10.11.2023

### Статус: Запропоновано

### Контекст

Застосунок "Звітність" передбачає роботу з файлами \*.xlsx. Це включає в себе як експорт так і імпорт звітів.
Нам потрібна відповідна бібліотека, яка може парсити та генерувати файли звітності навіть при відсутності встановленого MS Excel на робочій станції.
Враховуючі достатньо великий об'єм, та кількість звітів, з якими доведеться працювати оператору (принаймні в підрозділах рівня блигади та вище) критичним є фактор швидкості роботи бібліотеки.
Також враховуючі відсутність бюджету для придбання сторонніх компонентів бібліотека має бути доступною безкоштовно для некомерційного використання в державних установах.

### Рішення

У контексті потреби роботи з файлами \*.xlsx для забезпечення ефективного експорту та імпорту звітів у застосунку "Звітність", було прийнято рішення використовувати бібліотеку EPPlus. 
Ця бібліотека відповідає всім критеріям наших вимог:
EPPlus дозволяє парсити та генерувати файли Excel при відсутності встановленого MS Excel на робочій станції.
Здатна забезпечити швидку та ефективну обробку великого об'єму звітів.
З версії 5.х (поточна стабільна версія - 7.х) розповзюджується за ліцензію [PolyForm Noncommercial License 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0), 
яка дозволяє використовувати продукт безкоштовно для некомерційних державних установ.

### Наслідки

1. Висновок про легальність безкоштовного використання поточної ліцензії (PolyForm Noncommercial 1.0.0) в наших умовах був зроблений на основі відкритих даних.
Офіційне підтвердження від юристів відсутнє.
2. Даний продукт має історію зміни ліцензії в минулому (до в 4.х була LGPL 3.0), 
відповідно в разі зміни ліцензії в майбутньому і необхідності оновлення версії бібліотеки, це оновлення може бути ускладнене.

### Альтернативи

Були розглянуті наступні альтернативи:

| Технологія                                                                           | Працює без MSExcel | Зручність використання  | Активна підтримка  | Підтримка формул   | Швидкість роботи   | Можливість друку   | Вартість       |
|--------------------------------------------------------------------------------------|--------------------| ------------------------|--------------------|--------------------|--------------------|--------------------|----------------|
| [**Syncfusion**](https://www.syncfusion.com/document-processing/excel-framework/net) | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | **Висока (2100ms)**| :heavy_check_mark: | :heavy_check_mark: **Comunity edition** |
| [**Spire.Xls**](https://www.e-iceblue.com/Introduce/excel-for-net-introduce.html)    | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | **Висока (1800ms)**| :heavy_check_mark: | :heavy_check_mark: **Free edition with restrictions** |
| [EPPlus 7.x](https://www.epplussoftware.com)                                         | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | Висока (677ms)     | :x:                | :heavy_check_mark: [PolyForm Noncommercial 1.0.0](https://polyformproject.org/licenses/noncommercial/1.0.0)    |
| [NPOI](https://github.com/dotnetcore/NPOI)                                           | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | Висока (1700ms)    | :x:                | :heavy_check_mark: [Apache License 2.0](https://www.apache.org/licenses/LICENSE-2.0)    |
| [Aspose.Cells](https://docs.aspose.com/cells/net)                                    | :heavy_check_mark: | :heavy_check_mark:      | :heavy_check_mark: | :heavy_check_mark: | Висока             | :heavy_check_mark: | :x: Комерційна |
| [SmartXLS](https://www.smartxls.com/index.htm)                                       | :heavy_check_mark: | :heavy_multiplication_x:| :heavy_check_mark: | :heavy_check_mark: | Висока             | :x:                | :x: Commercial opensource      |
| OpenXML                                                                              | :heavy_check_mark: | :heavy_multiplication_x:| :heavy_check_mark: | :heavy_check_mark: | Висока             | :x:                | :heavy_check_mark: Безкоштовно |
| Office interop                                                                       | :x:                | :x:                     | :heavy_check_mark: | :heavy_check_mark: | :x: Низька         | :heavy_check_mark: | :heavy_check_mark: Безкоштовно |


