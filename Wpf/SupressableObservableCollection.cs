using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Peanut.Libs.Wpf {
    /// <summary>
    /// An observable collection that can supress notifications.<br/>
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class SupressableObservableCollection<T> : ObservableCollection<T> {
        /// <summary>
        /// If this property is set to true, then CollectionChanged and PropertyChanged events
        /// are not published. Furthermore, if collection changes occur while this property is set
        /// to true, then subsequently setting the property to false will cause a CollectionChanged
        /// event to be published with Action=Reset.<br/>
        /// This is designed for faster performance in cases where a large number of items are to be
        /// added or removed from the collection, especially including cases where the entire
        /// collection is to be replaced.<br/>
        /// The caller should follow this pattern:<br/>
        ///   1) Set NotificationSuppressed to true.<br/>
        ///   2) Do a number of Add, Insert, and/or Remove calls.<br/>
        ///   3) Set NotificationSuppressed to false.<br/>
        /// </summary>
        public bool NotificationSuppressed {
            get { return notificationSuppressed; }
            set {
                notificationSuppressed = value;
                if (notificationSuppressed is false && havePendingNotifications) {
                    OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
                    OnPropertyChanged(new PropertyChangedEventArgs("Count"));
                    OnCollectionChanged(
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    havePendingNotifications = false;
                }
            }
        }
        private bool notificationSuppressed = false;

        /// <summary>
        /// This field indicates whether there have been notifications that have been suppressed
        /// due to the NotificationSuppressed property having value of true.<br/>
        /// If this field is true, then when NotificationSuppressed is next set to false, a
        /// CollectionChanged event is published with Action=Reset, and the field is reset to false.<br/>
        /// </summary>
        private bool havePendingNotifications = false;

        /// <summary>
        /// Clears the collection but first tries to perform an action over the items.
        /// </summary>
        /// <param name="action">The action to handle the items before they get removed.</param>
        public void ClearWithAction(Action<IEnumerable<T>> action) {
            List<T> items = new(this);
            action(items);
            Clear();
        }

        /// <summary>
        /// This method publishes the CollectionChanged event with the provided arguments.<br/>
        /// </summary>
        /// <param name="e">container for arguments of the event that is published</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
            if (NotificationSuppressed) {
                havePendingNotifications = true;
                return;
            }
            base.OnCollectionChanged(e);
        }

        /// <summary>
        /// This method publishes the PropertyChanged event with the provided arguments.<br/>
        /// </summary>
        /// <param name="e">container for arguments of the event that is published</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs e) {
            if (NotificationSuppressed) {
                return;
            }
            base.OnPropertyChanged(e);
        }
    }
}
