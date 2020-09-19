using MetroAutomation.Model;
using MetroAutomation.ViewModel;
using System;
using System.ComponentModel;
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

        public EditableItemsViewModel()
        {
            Items = new BindableCollection<NameID>();
            itemsView = CollectionViewSource.GetDefaultView(Items);
            itemsView.Filter = FilterDelegate;
        }

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
        public EditableItemsViewModel(Func<T, IItemEditor<T>> getEditorDelegate)
            : base()
        {
            var names = LiteDBAdaptor.GetNames<T>();

            for (int i = 0; i < names.Length; i++)
            {
                NameID item = names[i];
                Items.Add(item);
            }

            GetEditorDelegate = getEditorDelegate;

            Items.GetInstanceDelegate = AddDelegate;
            Items.EditDelegate = EditDelegate;
            Items.GetCopyDelegate = GetCopyDelegate;
            Items.RemoveDelegate = RemoveDelegate;
        }

        public Func<T, IItemEditor<T>> GetEditorDelegate { get; }

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
            item.ID = 0;

            if (Edit(ref item))
            {
                return new NameID(item);
            }
            else
            {
                return null;
            }
        }

        private bool RemoveDelegate(NameID nameID)
        {
            LiteDBAdaptor.RemoveData<T>(nameID.ID);
            return true;
        }
    }
}
