﻿using GongSolutions.Wpf.DragDrop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace MetroAutomation.ViewModel
{
    public class BindableCollection<T> : ObservableCollection<T>, IDropTarget
    {
        private T selectedItem;
        private bool isAnySelected;

        static BindableCollection()
        {
            BindingOperations.CollectionRegistering += CollectionRegistering;
        }

        public BindableCollection()
        {
            SyncRoot = (this as ICollection).SyncRoot;
            AddCommand = new CommandHandler(Add);
            AddCopyCommand = new CommandHandler(AddCopy);
            RemoveCommand = new AsyncCommandHandler(Remove);
            EditCommand = new CommandHandler(Edit);

            GetInstanceDelegate = () => Activator.CreateInstance<T>();
            GetCopyDelegate = (item) => item.BinaryDeepClone();
            RemoveDelegate = (item) => Task.FromResult(true);
        }

        public BindableCollection(IEnumerable<T> source)
            : this()
        {
            if (source != null)
            {
                foreach (T item in source)
                {
                    Add(item);
                }
            }
        }

        public bool AllowDropBetweenCollections { get; set; }

        public object SyncRoot { get; }

        public T SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(SelectedItem)));
                IsAnySelected = SelectedItem != null;
            }
        }

        public bool IsAnySelected
        {
            get
            {
                return isAnySelected;
            }
            set
            {
                isAnySelected = value;
                OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsAnySelected)));
            }
        }

        public ICommand AddCommand { get; }

        public IAsyncCommand RemoveCommand { get; }

        public ICommand AddCopyCommand { get; }

        public ICommand EditCommand { get; }

        public bool CanAdd => GetInstanceDelegate != null;

        public bool CanCopy => GetCopyDelegate != null;

        public bool CanEdit => EditDelegate != null;

        public bool CanRemove => RemoveDelegate != null;

        public Func<T> GetInstanceDelegate { get; set; }

        public Func<T, T> GetCopyDelegate { get; set; }

        public Func<T, Task<bool>> RemoveDelegate { get; set; }

        public Func<T, T> EditDelegate { get; set; }

        private static void CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (e.Collection is BindableCollection<T> collection)
            {
                BindingOperations.EnableCollectionSynchronization(collection, collection.SyncRoot);
            }
        }

        protected override void ClearItems()
        {
            lock (SyncRoot)
            {
                base.ClearItems();
            }
        }

        protected override void InsertItem(int index, T item)
        {
            lock (SyncRoot)
            {
                base.InsertItem(index, item);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            lock (SyncRoot)
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (SyncRoot)
            {
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, T item)
        {
            lock (SyncRoot)
            {
                base.SetItem(index, item);
            }
        }

        private void Add()
        {
            T toAdd = GetInstanceDelegate();

            if (toAdd != null)
            {
                Add(toAdd);
                SelectedItem = toAdd;
            }
        }

        private void AddCopy()
        {
            if (SelectedItem != null)
            {
                var toAdd = GetCopyDelegate(SelectedItem);

                if (toAdd != null)
                {
                    Add(toAdd);
                    SelectedItem = toAdd;
                }
            }
        }

        private async Task Remove()
        {
            if (SelectedItem != null)
            {
                var index = IndexOf(SelectedItem);

                if (index >= 0 && await RemoveDelegate(SelectedItem))
                {
                    Remove(SelectedItem);

                    if (Count > 0)
                    {
                        if (index >= Count)
                        {
                            index = Count - 1;
                        }

                        SelectedItem = this[index];
                    }
                    else
                    {
                        SelectedItem = default;
                    }
                }
            }
        }

        private void Edit()
        {
            if (CanEdit && SelectedItem != null)
            {
                int index = IndexOf(SelectedItem);

                if (index >= 0)
                {
                    var edited = EditDelegate(SelectedItem);
                    this[index] = edited;
                    SelectedItem = edited;
                }
            }
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.Data is T tData)
            {
                if (Contains(tData))
                {
                    dropInfo.Effects = System.Windows.DragDropEffects.Move;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                }
                else if (AllowDropBetweenCollections)
                {
                    dropInfo.Effects = System.Windows.DragDropEffects.Move;
                    dropInfo.DropTargetAdorner = DropTargetAdorners.Insert;
                }
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            if (dropInfo.Data is T tData)
            {
                if (Contains(tData))
                {
                    var index = IndexOf(tData);

                    if (index != dropInfo.InsertIndex)
                    {
                        Insert(dropInfo.InsertIndex, tData);

                        if (dropInfo.InsertIndex < index)
                        {
                            index++;
                        }

                        RemoveAt(index);
                    }
                }
                else if (AllowDropBetweenCollections)
                {
                    Insert(dropInfo.InsertIndex, tData);
                    
                    if (dropInfo.DragInfo.SourceCollection is BindableCollection<T> tCollection)
                    {
                        tCollection.Remove(tData);
                    }
                }
            }
        }
    }
}
