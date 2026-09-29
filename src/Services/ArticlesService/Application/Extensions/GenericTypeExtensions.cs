namespace ArticlesService.Application.Extensions
{
    public static class GenericTypeExtensions
    {
        /*
         Это utility класс (расширение) для красивого форматирования имён генерических типов. Обычно это нужно для логирования, сообщений об ошибках и отладки.

        Проблема, которую решает:

            var type = typeof(OrderCreatedEvent);
            Console.WriteLine(type.Name);  
            // Вывод: "OrderCreatedEvent" ✓ Нормально

            var genericType = typeof(IIntegrationEventHandler<OrderCreatedEvent>);
            Console.WriteLine(genericType.Name);
            // Вывод: "IIntegrationEventHandler`1" ❌ Не читаемо!

        Пример результата выполнения:
        var type = typeof(IIntegrationEventHandler<OrderCreatedEvent>);

        type.Name
        // "IIntegrationEventHandler`1"  ← Contains `1 (` + количество параметров)

        type.Name.IndexOf('`')
        // 27 (индекс позиции символа `)

        type.Name.Remove(27)
        // "IIntegrationEventHandler"  ← Обрезали `1

        type.GetGenericArguments()
        // [OrderCreatedEvent]

        → Итог: "IIntegrationEventHandler<OrderCreatedEvent>"

        ___________________________________________________________________________
        Где это используется в eShop?
        В логировании и отладке:

        // В ProcessEvent (строка 193)
        logger.LogInformation($"Processing event: {eventName}");

        // Или где-то в обработчиках
        logger.LogInformation($"Handling {integrationEvent.GetGenericTypeName()}");

        */

        public static string GetGenericTypeName(this Type type)
        {
            string typeName;

            if (type.IsGenericType)
            {

                var genericTypes = string.Join(",",
                    type.GetGenericArguments()           // Получить все параметры типа   // Результат: [String, Int32]
                        .Select(t => t.Name)             // Взять имя каждого            // Результат: ["String", "Int32"]
                        .ToArray());                     // Преобразовать в массив для Join  // Конечный результат: "String,Int32" 

                // Удаление обратного апострофа и форматирование
                // Пример: "IIntegrationEventHandler`1" → "IIntegrationEventHandler" - удалили все, что шло после апострофа
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
                //           ↑                                      ↑
                //           Dictionary`2 → Dictionary              String,Int32
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }

        public static string GetGenericTypeName(this object @object)
        {
            return @object.GetType().GetGenericTypeName();
        }
    }
}
