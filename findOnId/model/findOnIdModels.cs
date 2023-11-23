using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace findOnId.model {
    class findOnIdModels : INotifyPropertyChanged {
        //INotifyPropertyChanged - добавляем событие на изменение данных он описан ниже
        //public DateTime CreationDate { get; set; } = DateTime.Now;
        private string _id;
        private int _numStart;
        public string id {
            get{return _id;}
            set{
                if (value == _id) return;
                _id = value;
                OnPropertyChanged("id");
            }
        }
        public int numStart {
            get { return _numStart; }
            set {
                if (value == numStart) return;
                _numStart = value;
                OnPropertyChanged("numStart");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
