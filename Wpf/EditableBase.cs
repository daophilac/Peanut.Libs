using System.ComponentModel;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// A subclass of the <see cref="BindableBase"/> class that implements
    /// <see cref="IEditableObject"/> to enable rollback machenism.<br/>
    /// </summary>
    public abstract class EditableBase<T> : IEditableObject {
        /// <summary>
        /// Gets a value indicating whether this object is in edit mode.<br/>
        /// </summary>
        public bool InEdit { get; private set; }

        #region IEditableObject implementation
#nullable disable
        private T backupCopy;
#nullable enable

        /// <summary>
        /// Begins editing the object.<br/>
        /// This method internally calls the <see cref="BeginEdit"/> method.<br/>
        /// </summary>
        void IEditableObject.BeginEdit() {
            if (InEdit) {
                return;
            }
            InEdit = true;
            backupCopy = Clone();
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
        protected virtual void BeginEdit(T snapshot) { }

        /// <summary>
        /// Occurs when the object cancels editing.<br/>
        /// </summary>
        /// <param name="snapshot">The snapshot of the object before it enters edit mode.</param>
        protected virtual void CancelEdit(T snapshot) { }

        /// <summary>
        /// Occurs when the object ends editing.<br/>
        /// </summary>
        /// <param name="snapshot">The snapshot of the object before it enters edit mode.</param>
        protected virtual void EndEdit(T snapshot) { }

        /// <summary>
        /// Clones the current editing object.<br/>
        /// This method will be called when the object enters edit mode.
        /// </summary>
        /// <returns>The cloned object.</returns>
        protected abstract T Clone();
    }
}
