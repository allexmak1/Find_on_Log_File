using findOnId.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace findOnId.Services {
    class ReadAndWrite {
        public delegate void Handler0();
        public event Handler0 eventEnd;

        public delegate void Handler1(int progress);
        public event Handler1 eventProgressBar;

        public delegate void Handler2(string fileName);
        public event Handler2 eventGetFileName;

        public delegate void Handler3(int MaxValue);
        public event Handler3 eventProgressBarMaxValue;

        private StreamReader reader;
        private StreamWriter writer;
        private List<string> _fileNames = new List<string>();
        private List<string> _safeFileNames = new List<string>();
        private BindingList<findOnIdModels> _idDataList;

        public ReadAndWrite(string[] fileNames, string[] safeFileNames, BindingList<findOnIdModels> idDataList) {
            _fileNames.AddRange(fileNames);
            _safeFileNames.AddRange(safeFileNames);
            _idDataList = idDataList;
        }

        public void ReadAndWriteThread() {
#if DEBUG
            // замерить время выполнения
            var sw = new Stopwatch();
            sw.Start();
#endif
            int cntName = 0;
            int progress, progressTotal;
            string line;
            string pattern = "\"\\w+\"(?=\\;\"(E|S))";
            Match match;

            foreach (string fName in _fileNames) {
                try {

                    progressTotal = (System.IO.File.ReadAllLines(fName).Length);
                    eventProgressBarMaxValue(System.IO.File.ReadAllLines(fName).Length);
                    progress = 0;
                    // запускаем событие на индикацию чтения нового файла 
                    if (eventGetFileName != null) eventGetFileName(_safeFileNames[cntName++]);

                    using (reader = new StreamReader(fName)) {
                        string nameResult = fName.Insert(fName.LastIndexOf("."), "_Result");
                        writer = new StreamWriter(File.Open(nameResult, FileMode.Create), Encoding.GetEncoding(1251));
                        //StreamWriter writer = File.CreateText(nameResult);//UTF8
                        UserParseLine Pl = new UserParseLine(writer);
                        while (!reader.EndOfStream) {
                            // парсим строку
                            line = reader.ReadLine();
                            if (!string.IsNullOrEmpty(line)) {
                                // поиск по таблице
                                for (int i = 0; i < _idDataList.Count; i++) {
                                    if (line.Length > _idDataList[i].numStart) {
                                        //nm = line.IndexOf(_idDataList[i].id, _idDataList[i].numStart);//так чуточку дольше
                                        match = Regex.Match(line, pattern, RegexOptions.Compiled);
                                        if (match.Success) {
                                            if (match.Value == _idDataList[i].id) {
                                                Pl.ParseLine(line, _idDataList[i].id, i);
                                            }
                                        }
                                    }
                                }
                            }
                            // запускаем событие на инкремент прогресса выполнения
                            if (eventProgressBar != null) eventProgressBar(++progress);
                        }
                        writer.Dispose();
                    }
                } catch (ThreadAbortException) {
                    //тк задачу мы отменили принудительно закроем файлы
                    reader.Dispose();
                    writer.Dispose();
                } catch (Exception e) {
                    //MessageBox.Show("Error: Файл занят другой программой \nException: " + e.Message + "_" + e.HelpLink + "_" + e.Source);
                    MessageBox.Show("Error: " + e.Message);
                }
            }
            // запускаем событие на окончание выполнения
            if (eventEnd != null) eventEnd();
            //eventEnd?.Invoke();//не работает в этой версии фреймворка
#if DEBUG
            sw.Stop();
            MessageBox.Show("Затрачено: " + sw.Elapsed);
#endif
        }


    }
}
