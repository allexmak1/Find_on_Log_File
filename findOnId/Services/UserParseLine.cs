using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace findOnId.Services {
    class UserParseLine {
        private StreamWriter _writer;
        private static bool isStart = false;
        private static bool isEnd = false;
        int zeroAdc = 0, rezult = 0;
        float energy = 0;
        double angle = 0;

        public UserParseLine(StreamWriter writer) {
            _writer = writer;
        }

        public void ParseLine(string line, string id, int indexLine) {

            // здесь можно добавить что то свое, к примеру:
            string addText;
            int pos, adc;

            if (indexLine == 0) {//103C940
                isStart = true;
                pos = line.IndexOf("\";\"", 27);
                if (pos < 0) return;
                addText = line.Substring(pos + 13, 2) + line.Substring(pos + 10, 2);
                angle = ((float)int.Parse(addText, System.Globalization.NumberStyles.HexNumber)) * 0.0054931640625;
            } else if (indexLine == 1) {//904201F
                if (isStart) isEnd = true;
                isStart = false;
            } else if (indexLine == 2) {//904A01F
                pos = line.IndexOf("\";\"", 27);
                if (pos < 0) return;
                addText = line.Substring(pos + 13, 2) + line.Substring(pos + 10, 2);
                energy = ((float)int.Parse(addText, System.Globalization.NumberStyles.HexNumber))/100;
            } else if (indexLine == 3) {//888
                pos = line.IndexOf("\";\"", 27);
                if (pos < 0) return;
                addText = line.Substring(pos + 6, 2) + line.Substring(pos + 3, 2);
                zeroAdc = int.Parse(addText, System.Globalization.NumberStyles.HexNumber);
                addText = line.Substring(pos + 21, 2);
                rezult = int.Parse(addText, System.Globalization.NumberStyles.HexNumber);
            } else if (isStart) {
                if (indexLine == 4) {//771
				pos = line.IndexOf("\";\"", 27);
                if (pos < 0) return;
                addText = line.Substring(pos + 6, 2) + line.Substring(pos + 3, 2);
                adc = int.Parse(addText, System.Globalization.NumberStyles.HexNumber);
                _writer.WriteLine(line + ";\"ADC:\";\"" + adc + "\"");
			}
            } else if (isEnd) {
                isEnd = false;
                _writer.WriteLine("\"ANGLE:\";\"\";\"\";\"\";\"\";\"\";\"" + angle + "\"");
                _writer.WriteLine("\"ZERO_ADC:\";\"\";\"\";\"\";\"\";\"\";\"" + zeroAdc + "\"");
                _writer.WriteLine("\"ENERGY:\";\"\";\"\";\"\";\"\";\"\";\"" + energy + "\"");
                switch (rezult) {
                    case 0:
                        _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"0 – Нет закусывания.\"");
                        break;
                    case 1:
                        _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"1 – Есть закусывание. Ударное центрирование снаряда. (Udar > 1)\"");
                        break;
                    case 2:
                        _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"2 – Есть закусывание. Ударное центрирование снаряда. (Udar > 0)\"");
                        break;
                    case 3:
                        _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"3 – Есть закусывание. (Udar <= 0)\"");
                        break;
                    case 4:
                        _writer.WriteLine("\"REZULT: 4 – Нет закусывания! Удар в каморе! Неисправность досылателя! (Udar > 0)\"");
                        break;
                    case 5:
                        _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"5 – Превышено время досылки! Нет закусывания! Ударов требуемой энергетики не выявлено! Неисправность досылателя!\"");
                        break;
                    default:
                        _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"error\"");
                        break;
                }
            }

        }
    }
}
