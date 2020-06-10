using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetProtocol.MessageProcessors
{
    /// <summary>
    /// Интерфейс для ассинхронной обработки очереди пакетов
    /// </summary>
    public interface IPackageQueueProcessor<T>
    {
        /// <summary>
        /// Запуск обработки сообщений
        /// </summary>
        void Start();

        /// <summary>
        /// Остановка обработки сообщений
        /// </summary>
        void Stop();

        /// <summary>
        /// Возвращает флаг работы процессора
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Добавление нового объекта <see cref="T"/> в очередь для обработки
        /// </summary>
        /// <param name="dataItem">Объект с данными</param>
        void Add(T dataItem);

        /// <summary>
        /// Количество еще необработанных сообщений
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Метод для обработки объектов типа <see cref="T"/>
        /// </summary>
        Action<T> ProcessAction { get; }
    }
}
