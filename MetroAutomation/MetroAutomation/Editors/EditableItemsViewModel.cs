﻿using MahApps.Metro.Controls;
using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MetroAutomation.Editors
{
    public interface IEditable
    {
        public bool IsEditing { get; }

        public void OnBeginEdit();

        public void OnEndEdit();
    }

    public interface IItemEditor<T>
    {
        public T Item { get; }

        public bool? ShowDialog();
    }

    public class EditableItemsViewModel
    {
        private readonly ICollectionView itemsView;
        private string filter;

        public EditableItemsViewModel(NameID[] source)
        {
            Items = new BindableCollection<NameID>(source);
            itemsView = CollectionViewSource.GetDefaultView(Items);
            itemsView.Filter = FilterDelegate;
        }

        public MetroWindow Owner { get; set; }

        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                itemsView.Refresh();
            }
        }

        public BindableCollection<NameID> Items { get; }

        private bool FilterDelegate(object arg)
        {
            if (string.IsNullOrWhiteSpace(Filter))
            {
                return true;
            }
            else
            {
                if (arg is NameID nameID && !string.IsNullOrEmpty(nameID.Name))
                {
                    return nameID.Name.Contains(Filter ?? string.Empty, StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    return true;
                }
            }
        }
    }

    public class EditableItemsViewModel<T> : EditableItemsViewModel where T : class, IDataObject, IEditable, new()
    {
        public EditableItemsViewModel(Func<T, IItemEditor<T>> getEditorDelegate, Func<MetroWindow, NameID, Task<bool>> removeDelegate)
            : base(LiteDBAdaptor.GetNames<T>())
        {
            GetEditorDelegate = getEditorDelegate;
            RemoveDelegate = removeDelegate;

            Items.GetInstanceDelegate = AddDelegate;
            Items.EditDelegate = EditDelegate;
            Items.GetCopyDelegate = GetCopyDelegate;
            Items.RemoveDelegate = Remove;
        }

        public Func<T, IItemEditor<T>> GetEditorDelegate { get; }

        public Func<MetroWindow, NameID, Task<bool>> RemoveDelegate { get; }

        private NameID AddDelegate()
        {
            var item = new T();

            if (Edit(ref item))
            {
                return new NameID(item);
            }
            else
            {
                return null;
            }
        }

        private NameID EditDelegate(NameID nameID)
        {
            var item = LiteDBAdaptor.LoadData<T>(nameID.ID);
            Edit(ref item);
            return new NameID(item);
        }

        private bool Edit(ref T item)
        {
            var cloned = item.BinaryDeepClone();
            cloned.OnBeginEdit();

            var editor = GetEditorDelegate(cloned);

            if (editor.ShowDialog() == true)
            {
                var result = editor.Item;
                result.OnEndEdit();

                LiteDBAdaptor.SaveData(result);
                item = editor.Item;
                return true;
            }
            else
            {
                return false;
            }
        }

        private NameID GetCopyDelegate(NameID nameID)
        {
            var item = LiteDBAdaptor.LoadData<T>(nameID.ID);
            item.ID = Guid.Empty;

            if (Edit(ref item))
            {
                return new NameID(item);
            }
            else
            {
                return null;
            }
        }

        private async Task<bool> Remove(NameID nameID)
        {
            if (await (RemoveDelegate?.Invoke(Owner, nameID) ?? Task.FromResult(false)))
            {
                LiteDBAdaptor.RemoveData<T>(nameID.ID);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
