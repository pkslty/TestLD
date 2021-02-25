using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Modbus.Device;
using System.IO.Ports;
using System.Threading;


namespace WindowsFormsApp1
{
    class modbusMaster
    {
        const int TIMEOUT = 23000;
        int timeout;
        
        SerialPort serialPort;
        ModbusSerialMaster master;
        byte slaveID = 1;
        ushort startAddress = 0;
        ushort numOfPoints = 10;
        ushort coilAddress = 2;
        public ushort[] holding_register;
        public bool error;
        public string errorText;
        public bool ready;

        
        void errorDecoder(int error)
        {
            if (error == 0)
            {
                errorText = "Ошибок нет";
            }
            else
            {
                errorText = "ОШИБКА:";
                if (Convert.ToBoolean(error & 1))
                    errorText += " Отсутствуют колебания 1-го датчика";
                if (Convert.ToBoolean(error & 2))
                    errorText += " Отсутствуют колебания 2-го датчика";
                if (Convert.ToBoolean(error & 4))
                    errorText += " Шум по первому каналу";
                if (Convert.ToBoolean(error & 8))
                    errorText += " Шум по второму каналу";
            }
            
        }
        public modbusMaster(int tensNum)
        {
            this.serialPort = new SerialPort(); 
            serialPort.PortName = "COM1";
            serialPort.BaudRate = 4800;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;
            this.ready = false;
            this.errorText = "Ошибок нет";
            this.error = false;
            this.holding_register = new ushort[numOfPoints];
        }

        public void setParameters(string com, int tensNum)
        {
            holding_register[0] = 0;
            if (!ready)
            {
                serialPort.PortName = com;
                switch (tensNum)
                {
                    case 1:
                        {
                            this.coilAddress = 1;
                            timeout = TIMEOUT;
                            error = false;
                            break;
                        }
                    case 2:
                        {
                            this.coilAddress = 2;
                            timeout = TIMEOUT;
                            error = false;
                            break;
                        }
                    case 3:
                        {
                            this.coilAddress = 0;
                            timeout = 2 * TIMEOUT;
                            error = false;
                            break;
                        }
                    default:
                        {
                            ready = false;
                            error = true;
                            errorText = "ОШИБКА: Не выбрано ни одного датчика";
                            break;
                        }
                }
            } else
            {
                serialPort.Close();
                ready = false;
                setParameters(com, tensNum);
            }
        }

        public bool ReadyToMeasure()
        {
            if (!ready)
            {
                if (error)
                    return false;
                try
                {
                    serialPort.Open();
                }
                catch (System.ArgumentException)
                {
                    errorText = "ОШИБКА: Неправильные настройки или имя порта";
                    error = true;
                    return false;
                }
                catch (System.IO.IOException)
                {
                    errorText = "ОШИБКА: Порт " + serialPort.PortName + " не существует";
                    error = true;
                    return false;
                }
                
                catch (System.UnauthorizedAccessException)
                {
                    errorText = "Порт уже открыт";
                    error = true;
                    return false;
                }
                this.master = ModbusSerialMaster.CreateRtu(serialPort);
                master.Transport.ReadTimeout = timeout;
                holding_register = master.ReadHoldingRegisters(slaveID, startAddress, numOfPoints);
                if (holding_register[0] != 111)
                {
                    errorText = "ОШИБКА: К порту " + serialPort.PortName + " не подключен регистратор линейных деформаций";
                    error = true;
                    this.ready = true;
                    return false;
                }
                this.ready = true;
                errorText = "Выполняется измерение";
                error = false;
                return true;
            }
            return true;
        }

        public void takeAMeasure()
        {
            try
            {
                master.WriteSingleCoil(slaveID, coilAddress, true);
                holding_register = master.ReadHoldingRegisters(slaveID, startAddress, numOfPoints);
                errorDecoder(holding_register[5]);
            }
            catch (System.TimeoutException)
            {
                errorText = "ОШИБКА: Превышено время ожидания";
                error = true;
            }
        }
    }
}
