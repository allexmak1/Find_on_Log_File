using findOnId.model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace findOnId.Services {
    class ReadAndWrite {
        public delegate void Handler0();
        public event Handler0 eventEnd;

        public delegate void Handler1(int progress, int progressTotal);
        public event Handler1 eventProgressBar;

        public delegate void Handler2(string fileName);
        public event Handler2 eventGetFileName;

        List<string> _fileNames = new List<string>();
        List<string> _safeFileNames = new List<string>();
        private BindingList<findOnIdModels> _idDataList;

        //public ReadAndWrite(string fileName, BindingList<findOnIdModels> idDataList) {
        public ReadAndWrite(string[] fileNames, string[] safeFileNames, BindingList<findOnIdModels> idDataList) {
            _fileNames.AddRange(fileNames);
            _safeFileNames.AddRange(safeFileNames);
            _idDataList = idDataList;
        }

        public void ReadAndWriteThread() {
            // для теста
            //int progress = 0;
            //int progressTotal = 200;
            //for (int i = 0; i < 200; i++) {
            //    progress = i;
            //    Thread.Sleep(50);     // имитация продолжительной работы
            //    ProgressBar(progress, progressTotal);
            //}

            int nm, progress, progressTotal, cntName = 0;
            string line;

            foreach (string fName in _fileNames) {
                try {
                    progressTotal = (System.IO.File.ReadAllLines(fName).Length);
                    progress = 0;
                    // запускаем событие на индикацию чтения нового файла 
                    if (eventGetFileName != null) eventGetFileName(_safeFileNames[cntName++]);

                    using (StreamReader reader = new StreamReader(fName)) {
                        string nameResult = fName.Insert(fName.LastIndexOf("."), "_Result");
                        StreamWriter writer = new StreamWriter(File.Open(nameResult, FileMode.Create), Encoding.GetEncoding(1251));
                        //StreamWriter writer = File.CreateText(nameResult);//UTF8
                        UserParseLine Pl = new UserParseLine(writer);
                        while (!reader.EndOfStream) {
                            // парсим строку
                            line = reader.ReadLine();
                            if (!string.IsNullOrEmpty(line)) {
                                // поиск по таблице
                                for (int i = 0; i < _idDataList.Count; i++) {
                                    if (line.Length > _idDataList[i].numStart) {
                                        nm = line.IndexOf(_idDataList[i].id, _idDataList[i].numStart);
                                        if (nm > 0) {
                                            Pl.ParseLine(line, _idDataList[i].id, i);
                                        }
                                    }
                                }
                            }
                            // запускаем событие на инкремент прогресса выполнения
                            if (eventProgressBar != null) eventProgressBar(++progress, progressTotal);
                        }
                        writer.Dispose();
                    }
                } catch (Exception e) {
                    MessageBox.Show("Error: Файл занят другой программой \nException: " + e.Message);
                }
            }
            // запускаем событие на окончание выполнения
            if (eventEnd != null) eventEnd();
            //eventEnd?.Invoke();//не работает в этой версии фреймворка
        }
    }
}
