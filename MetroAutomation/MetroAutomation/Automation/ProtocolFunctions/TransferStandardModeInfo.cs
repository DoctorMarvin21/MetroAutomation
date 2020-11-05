using System;
using System.Collections.Generic;
using System.Text;

namespace MetroAutomation.Automation
{
    public class TransferStandardSetModeInfo : PairedModeInfo
    {
        // 0. Отключить выход калибратора
        // 1. Найти максимальный диапазон трансферного стандарта для значения напряжения AC
        // 2. Всплывающее сообщение, что нужно установить диапазон на трансферном стандарте (использовать делитель 1/100)
        // 3. Установить калибратор в режим DC
        // 4. Установить вольтметр в режим DC на диапазон, содержащий 2 В (можно, конечно, взять уже заданый, но это будет не очень верно)
        // 5. Включить выход калибратора
        // 6. Измерить опорное напряжение DC
        // 7. Выключить выход калибратора
        // 8. Установить калабратор в режим AC
        // 9. Включить выход калибратора
        // 10. Измерить номинальное напряжение
        // 11. Ввести измереное значение как AC + DC - NOMINAL
        // 12. При поверке вольтметра в режиме АС использовать это значение как реально установленное

        public override DeviceProtocolItem GetProtocolRow(DeviceProtocolBlock block)
        {
            return base.GetProtocolRow(block);
        }

        public override DeviceProtocolItem GetProtocolRowCopy(DeviceProtocolBlock block, DeviceProtocolItem source, RowLoadMode loadMode)
        {
            return base.GetProtocolRowCopy(block, source, loadMode);
        }
    }
}
