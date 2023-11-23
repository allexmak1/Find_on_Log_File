using System;
using System.ComponentModel;
using findOnId.model;
using System.IO;

namespace findOnId.Services {
    class FileIOService {
        private readonly string PATH;

        public FileIOService(string path) {
            PATH = path;
        }

        // загружаем из файла
        public BindingList<findOnIdModels> LoadData() {
            bool fileExists = File.Exists(PATH);
            if (!fileExists) {
                File.CreateText(PATH).Dispose();
                return new BindingList<findOnIdModels>();
            }
            //отказался от Json, нет возможности подключить библиотеку
            /*using (var reader = File.OpenText(PATH)) {
                var fileText = reader.ReadToEnd();
                return JsonConvert.DeserializeObject<BindingList<findOnIdModels>>(fileText);
            }*/
            bool error = false;
            BindingList<findOnIdModels> _idDataList;
            _idDataList = new BindingList<findOnIdModels>();
            using (StreamReader read = new StreamReader(PATH)) {
                int i = 0;
                string line;
                while (!read.EndOfStream) {
                    //разбиваем строку
                    while ((line = read.ReadLine()) != null) {
                        if (line != "") {
                            string[] words = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            var newPart = _idDataList.AddNew();
                            try {// если строка некорректна то пропустим ее и перезапишем конфиг
                                _idDataList[i].numStart = Int32.Parse(words[1]);
                                _idDataList[i].id = words[0];
                                i++;
                            } catch (Exception ex) {
                                _idDataList.CancelNew(_idDataList.IndexOf(newPart));
                                error = true;
                                //MessageBox.Show("Error on " + PATH + "\nException: " + ex.Message);
                            }
                        }
                    }
                }
            }
            if (error) SaveData(_idDataList);
            return _idDataList;
        }

        //сохраняем конфиг
        public void SaveData(BindingList<findOnIdModels> _idDataList) {//если object то не поучается обратится
            using (StreamWriter writer = File.CreateText(PATH)) {
                //отказался от Json, нет возможности подключить библиотеку
                /*string DataLine = JsonConvert.SerializeObject(_idDataList); 
                writer.Write(DataLine);*/
                string DataLine = "";
                for (int i = 0; i < _idDataList.Count; i++ ) {
                    DataLine = _idDataList[i].id + " " + _idDataList[i].numStart/* + "\n"*/;
                    writer.WriteLine(DataLine);
                }
            }
        }


    }
}
