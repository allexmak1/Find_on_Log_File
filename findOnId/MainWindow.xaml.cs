#define LOCK_TABLE

using System;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;
using findOnId.model;
using findOnId.Services;
using Microsoft.Win32;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Collections.Concurrent;
using System.Text;

namespace findOnId {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly string PATH_CFG = @"config.cfg";
        private BindingList<findOnIdModels> _idDataList;
        private FileIOService _fileConfig;
        private bool isClickStart = false;

        private System.Threading.CancellationTokenSource tokenSource = new CancellationTokenSource();
        Thread _writeThread;

        public MainWindow() {
            InitializeComponent();
        }

        //событие после загрузки экрана
        private void Window_Loaded(object sender, RoutedEventArgs e) {
            _fileConfig = new FileIOService(PATH_CFG);

            //загружаем из файла config.cfg сохраненные параметры
            try {
                _idDataList = _fileConfig.LoadData();
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
                Close();
            }
            if (_fileConfig.getFileExists())
                tStatusBar.Text = "Открыто сохранение";
            else
                tStatusBar.Text = "Сохранение не найденно, создаю пустое";
            //добавляем стартовые
            //_idDataList = new BindingList<findOnIdModels>(){
            //    new findOnIdModels(){id = "0x100000", numStart = 15},
            //    new findOnIdModels(){id = "0x200000", numStart = 15},
            //};
            dgListId.ItemsSource = _idDataList;//привязали список выше с используя BindingList
            _idDataList.ListChanged += _idDataList_ListChanged;//подпишемся на событие изменеия
            if (UserParseLine.LOCK_TABLE) {
                dgListId.IsEnabled = false;
            }
        }
        
        //событие на изменение таблицы
        private void _idDataList_ListChanged(object sender, ListChangedEventArgs e) {
            // добавляем элементы по умолчанию
            if (e.ListChangedType == ListChangedType.ItemAdded) {
                _idDataList[_idDataList.Count - 1].id = "0";
                _idDataList[_idDataList.Count - 1].numStart = 15;
            }
            if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted || e.ListChangedType == ListChangedType.ItemChanged) {
                //переберем весь диапазон если в нем есть пустое то запишим что то по умолчанию
                for (int i = 0; i < _idDataList.Count; i++) {
                    if (_idDataList[i].id == "") _idDataList[i].id = "0";
                }

                //сохраняем
                try {
                    //_fileIOService.SaveDa(sender);
                    _fileConfig.SaveData(_idDataList);
                } catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                    Close();
                }
            }
        }

        //кнопка открыть/остановить
        private void button1_Click(object sender, RoutedEventArgs e) {
            if (!isClickStart) {
                isClickStart = true;
                button1.Content = "Остановить";
                // диалоговое окно
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CSV files (*.CSV)|*.CSV|All files (*.*)|*.*";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == false) {
                    tStatusBar.Text = "Файл не выбран";
                    button1.Content = "Открыть";
                    isClickStart = false;
                    return;
                }
                progressBar1.Visibility = Visibility;
                // работаем с в другом потоке
                ReadAndWrite _ReadAndWrite = new ReadAndWrite(openFileDialog.FileNames, openFileDialog.SafeFileNames, _idDataList);
                _ReadAndWrite.eventEnd += eventThreadEnd;
                _ReadAndWrite.eventProgressBar += eventProgressBar;
                _ReadAndWrite.eventProgressBarMaxValue += eventProgressBarMaxValue;
                _ReadAndWrite.eventGetFileName += eventThatFile;
                _writeThread = new Thread(new ThreadStart(_ReadAndWrite.ReadAndWriteThread));
                _writeThread.Start();

            } else {
                //экстренно завершим задачу
                _writeThread.Abort();
                _writeThread.Join();
                tStatusBar.Text = "Отменено!";
                button1.Content = "Открыть";
                isClickStart = false;
            }
        }

        //событие на чтение нового файла
        private void eventThatFile(string FileName) {
            this.Dispatcher.Invoke((Action)(() => {
                tStatusBar.Text = "Обработка \"" + FileName + "\"";
            }));
        }

        //событие на завершение потока
        private void eventThreadEnd() {
            this.Dispatcher.Invoke((Action)(() => {
                tStatusBar.Text = "Готово!";
                button1.Content = "Открыть";
                isClickStart = false;
            }));
        }

        //событие на заполнение прогресс бара
        private void eventProgressBar(int progress) {
            //on WinForm
            //progressBar1.Invoke(new Action(() => 
            //{ 
            //    progressBar1.Value = (((double)progress / progressTotal) * 100)+1;
            //}));
            //on WPF
            this.Dispatcher.Invoke((Action)(() => {
                progressBar1.Value = progress + 1;
            }));
        }
        //установка максимального значения progressBar1
        private void eventProgressBarMaxValue(int maxValue) {
            this.Dispatcher.Invoke((Action)(() => {
                progressBar1.Maximum = maxValue;
            }));
        }
        //событие на закрытие окна
        private void Window_Closing(object sender, CancelEventArgs e) {
            if (_writeThread != null) {
                if (_writeThread.IsAlive) {
                    _writeThread.Abort(); // запускаем исключение catch (ThreadAbortException)
                    _writeThread.Interrupt();
                }
            }
        }
        
    }
}
