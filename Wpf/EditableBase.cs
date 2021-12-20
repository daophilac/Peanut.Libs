using System.ComponentModel;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// A subclass of the <see cref="BindableBase"/> class that implements
    /// <see cref="IEditableObject"/> to enable rollback machenism.<br/>
    /// </summary>
    public abstract class EditableBase : BindableBase, IEditableObject {
        /// <summary>
        /// Gets a value indicating whether this object is in edit mode.<br/>
        /// </summary>
        public bool InEdit { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditableBase"/> class.<br/>
        /// </summary>
        public EditableBase() {
            backupCopy = (EditableBase)MemberwiseClone();
        }

        #region IEditableObject implementation
        private EditableBase backupCopy;

        /// <summary>
        /// Begins editing the object.<br/>
        /// This method internally calls the <see cref="BeginEdit"/> method.<br/>
        /// </summary>
        void IEditableObject.BeginEdit() {
            if (InEdit) {
                return;
            }
            InEdit = true;
            backupCopy = (EditableBase)MemberwiseClone();
            BeginEdit(backupCopy);
        }

        /// <summary>
        /// Cancels editing the object.<br/>
        /// This method internally calls the <see cref="CancelEdit"/> method.<br/>
        /// </summary>
        void IEditableObject.CancelEdit() {
            if (!InEdit) {
                return;
            }
            InEdit = false;
            CancelEdit(backupCopy);
        }

        /// <summary>
        /// Ends editing the object.<br/>
        /// This method internally calls the <see cref="EndEdit"/> method.<br/>
        /// </summary>
        void IEditableObject.EndEdit() {
            if (!InEdit) {
                return;
            }
            InEdit = false;
            EndEdit(backupCopy);
        }
        #endregion

        /// <summary>
        /// Occurs when the object begins editing.<br/>
        /// </summary>
        /// <param name="snapshot">The snapshot of the object before it enters edit mode.</param>
        protected virtual void BeginEdit(EditableBase snapshot) { }

        /// <summary>
        /// Occurs when the object cancels editing.<br/>
        /// </summary>
        /// <param name="snapshot">The snapshot of the object before it enters edit mode.</param>
        protected virtual void CancelEdit(EditableBase snapshot) { }

        /// <summary>
        /// Occurs when the object ends editing.<br/>
        /// </summary>
        /// <param name="snapshot">The snapshot of the object before it enters edit mode.</param>
        protected virtual void EndEdit(EditableBase snapshot) { }
    }
}
