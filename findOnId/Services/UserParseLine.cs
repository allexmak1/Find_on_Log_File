using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace findOnId.Services {
    class UserParseLine {

        public const bool LOCK_TABLE = false;//заблочить таблицу?

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
            if (LOCK_TABLE) {
                // здесь можно добавить что то свое, к примеру:
                // но незабыть отредактировать config.cfg
                string tempStr;
                int pos, adc;

                if (indexLine == 0) {//103C940
                    isStart = true;
                    pos = line.IndexOf("\";\"", 27);
                    if (pos < 0) return;
                    tempStr = line.Substring(pos + 13, 2) + line.Substring(pos + 10, 2);
                    angle = ((float)int.Parse(tempStr, System.Globalization.NumberStyles.HexNumber)) * 0.0054931640625;
                } else if (indexLine == 1) {//904201F
                    if (isStart) isEnd = true;
                    isStart = false;
                } else if (indexLine == 2) {//904A01F
                    pos = line.IndexOf("\";\"", 27);
                    if (pos < 0) return;
                    tempStr = line.Substring(pos + 13, 2) + line.Substring(pos + 10, 2);
                    energy = ((float)int.Parse(tempStr, System.Globalization.NumberStyles.HexNumber)) / 100;
                } else if (indexLine == 3) {//888
                    pos = line.IndexOf("\";\"", 27);
                    if (pos < 0) return;
                    tempStr = line.Substring(pos + 6, 2) + line.Substring(pos + 3, 2);
                    zeroAdc = int.Parse(tempStr, System.Globalization.NumberStyles.HexNumber);
                    tempStr = line.Substring(pos + 21, 2);
                    rezult = int.Parse(tempStr, System.Globalization.NumberStyles.HexNumber);
                } else if (isStart) {
                    if (indexLine == 4) {//771
                        //pos = line.IndexOf("\";\"", 27);
                        //if (pos < 0) return;
                        //tempStr = line.Substring(pos + 6, 2) + line.Substring(pos + 3, 2);
                        //regex:
                        adc = 0;
                        Regex regex = new Regex(@"\S*(?<byte0>[0-F]{2})\s(?<byte1>[0-F]{2})\s.+", RegexOptions.Compiled);
                        if (regex.IsMatch(line)) {
                            tempStr = regex.Replace(line, @"${byte1}${byte0}");
                            adc = int.Parse(tempStr, System.Globalization.NumberStyles.HexNumber);
                        }
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
                            _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"4 – Нет закусывания! Удар в каморе! Неисправность досылателя! (Udar > 0)\"");
                            break;
                        case 5:
                            _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"5 – Превышено время досылки! Нет закусывания! Ударов требуемой энергетики не выявлено! Неисправность досылателя!\"");
                            break;
                        default:
                            _writer.WriteLine("\"REZULT:\";\"\";\"\";\"\";\"\";\"\";\"error\"");
                            break;
                    }
                }
            } else {
                _writer.WriteLine(line);
            }
        }
        private string ParseLine(string line) {
            //можно написать универсальный метод, вроде этого:
            Regex regex = new Regex("(?<=\";\")[0-F]{2}(?=\\s)|(?<=\\s)[0-F]{2}", RegexOptions.Compiled);
            MatchCollection matches = regex.Matches(line);
            if (matches.Count > 0) {
                //выводим 0й к примеру
                return matches[0].Value;
            } else {
                return null;
            }
        }
    }
}
