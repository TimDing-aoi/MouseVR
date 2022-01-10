﻿using UnityEditor.IMGUI.Controls;

using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;

namespace Codice.Views.PendingChanges
{
    internal class ChangeTreeViewItem : TreeViewItem
    {
        internal PendingChangeInfo ChangeInfo { get; private set; }

        internal ChangeTreeViewItem(int id, PendingChangeInfo change)
            : base(id, 1)
        {
            ChangeInfo = change;

            displayName = change.GetColumnText(PlasticLocalization.GetString(
                PlasticLocalization.Name.ItemColumn));
        }
    }
}
