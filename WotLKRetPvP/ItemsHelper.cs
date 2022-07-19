using System;
using System.Threading;
using wManager.Wow.Helpers;

// Token: 0x02000005 RID: 5
public class ItemsHelper
{
	// Token: 0x06000020 RID: 32 RVA: 0x000035C0 File Offset: 0x000017C0
	public static float GetItemCooldown(string itemName)
	{
		string command = "\r\n        for bag=0,4 do\r\n            for slot=1,36 do\r\n                local itemLink = GetContainerItemLink(bag,slot);\r\n                if (itemLink) then\r\n                    local itemString = string.match(itemLink, \"item[%-?%d:]+\");\r\n                    if (GetItemInfo(itemString) == \"" + itemName + "\") then\r\n                        local start, duration, enabled = GetContainerItemCooldown(bag, slot);\r\n                        if enabled == 1 and duration > 0 and start > 0 then\r\n                            return (duration - (GetTime() - start));\r\n                        end\r\n                    end\r\n                end;\r\n            end;\r\n        end\r\n        return 0;";
		return Lua.LuaDoString<float>(command, "");
	}

	// Token: 0x06000021 RID: 33 RVA: 0x000035F0 File Offset: 0x000017F0
	public static float GetItemCooldown(uint id)
	{
		return ItemsHelper.GetItemCooldown(ItemsManager.GetNameById(id));
	}

	// Token: 0x06000022 RID: 34 RVA: 0x00003610 File Offset: 0x00001810
	public static void DeleteItems(string itemName, int leaveAmount = 0)
	{
		int num = ItemsManager.GetItemCountByNameLUA(itemName) - leaveAmount;
		bool flag = string.IsNullOrWhiteSpace(itemName) || num <= 0;
		if (!flag)
		{
			string command = string.Format("\r\n            local itemCount = {0}; \r\n            local deleted = 0; \r\n            for b=0,4 do \r\n                if GetBagName(b) then \r\n                    for s=1, GetContainerNumSlots(b) do \r\n                        local itemLink = GetContainerItemLink(b, s) \r\n                        if itemLink then \r\n                            local itemString = string.match(itemLink, \"item[%-?%d:]+\");\r\n                            local _, stackCount = GetContainerItemInfo(b, s);\r\n                            local leftItems = itemCount - deleted; \r\n                            if ((GetItemInfo(itemString) == \"{1}\") and leftItems > 0) then \r\n                                if stackCount <= 1 then \r\n                                    PickupContainerItem(b, s); \r\n                                    DeleteCursorItem(); \r\n                                    deleted = deleted + 1; \r\n                                else \r\n                                    if (leftItems > stackCount) then \r\n                                        SplitContainerItem(b, s, stackCount); \r\n                                        DeleteCursorItem(); \r\n                                        deleted = deleted + stackCount; \r\n                                    else \r\n                                        SplitContainerItem(b, s, leftItems); \r\n                                        DeleteCursorItem(); \r\n                                        deleted = deleted + leftItems; \r\n                                    end \r\n                                end\r\n                            end \r\n                        end \r\n                    end \r\n                end \r\n            end\r\n        ", num, itemName);
			Lua.LuaDoString(command, false);
		}
	}

	// Token: 0x06000023 RID: 35 RVA: 0x0000365C File Offset: 0x0000185C
	public static int GetItemCountSave(uint itemId)
	{
		int itemCountById = ItemsManager.GetItemCountById(itemId);
		bool flag = itemCountById > 0;
		int result;
		if (flag)
		{
			result = itemCountById;
		}
		else
		{
			Thread.Sleep(250);
			result = ItemsManager.GetItemCountById(itemId);
		}
		return result;
	}

	// Token: 0x06000024 RID: 36 RVA: 0x00003694 File Offset: 0x00001894
	public static int GetItemCountSave(string itemName)
	{
		int itemCount = ItemsHelper.GetItemCount(itemName);
		bool flag = itemCount > 0;
		int result;
		if (flag)
		{
			result = itemCount;
		}
		else
		{
			Thread.Sleep(250);
			result = ItemsHelper.GetItemCount(itemName);
		}
		return result;
	}

	// Token: 0x06000025 RID: 37 RVA: 0x000036CC File Offset: 0x000018CC
	public static int GetItemCount(string itemName)
	{
		string command = "\r\n        local fullCount = 0;\r\n        for bag=0,4 do\r\n            for slot=1,36 do\r\n                local itemLink = GetContainerItemLink(bag, slot);\r\n                if (itemLink) then\r\n                    local itemString = string.match(itemLink, \"item[%-?%d:]+\");\r\n                    if (GetItemInfo(itemString) == \"" + itemName + "\") then\r\n                        local texture, count = GetContainerItemInfo(bag, slot);\r\n                        fullCount = fullCount + count;\r\n                    end\r\n                end\r\n            end\r\n        end\r\n        return fullCount;";
		return Lua.LuaDoString<int>(command, "");
	}
}
