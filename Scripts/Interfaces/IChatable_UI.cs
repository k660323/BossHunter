using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IChatable_UI
{
    public UI_Chat Chat { get; set; }

    public void UpdateChat(string message)
    {
        Chat.UpdateChat(message);
    }
}
