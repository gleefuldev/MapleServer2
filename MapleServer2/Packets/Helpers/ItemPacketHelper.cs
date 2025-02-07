﻿using Maple2Storage.Enums;
using MaplePacketLib2.Tools;
using MapleServer2.Types;

namespace MapleServer2.Packets.Helpers;

public static class ItemPacketHelper
{
    public static PacketWriter WriteItem(this PacketWriter pWriter, Item item)
    {
        pWriter.WriteInt(item.Amount);
        pWriter.WriteInt();
        pWriter.WriteInt(-1);
        pWriter.WriteLong(item.CreationTime);
        pWriter.WriteLong(item.ExpiryTime);
        pWriter.WriteLong();
        pWriter.WriteInt(item.TimesAttributesChanged);
        pWriter.WriteInt(item.PlayCount);
        pWriter.WriteBool(item.IsLocked);
        pWriter.WriteLong(item.UnlockTime);
        pWriter.WriteShort(item.RemainingGlamorForges);
        pWriter.WriteByte();
        pWriter.WriteInt(item.GachaDismantleId);

        // Write Appearance 
        pWriter.WriteAppearance(item);

        // Write Stats 0x0582B10
        pWriter.WriteStats(item.Stats);
        pWriter.WriteInt(item.EnchantLevel);
        pWriter.WriteInt(item.EnchantExp);
        pWriter.WriteBool(true); // Enchant based peachy charges, otherwise always require 10 charges
        pWriter.WriteLong();
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteBool(item.TransferFlag.HasFlag(ItemTransferFlag.Tradeable) || item.RemainingTrades > 0 || item.RemainingRepackageCount > 0);
        pWriter.WriteInt(item.Charges);
        pWriter.WriteEnchantStats(item);

        if (item.IsCustomScore)
        {
            pWriter.WriteMusicScore(item);
        }

        if (item.Ugc != null)
        {
            pWriter.WriteTemplate(item.Ugc);
        }

        if (item.InventoryTab == InventoryTab.Pets)
        {
            pWriter.WritePet();
        }

        if (item.GemSlot != 0)
        {
            // Now deviate from WriteItem
            pWriter.WriteBool(true);
            pWriter.WriteByte((byte) item.GemSlot);
            pWriter.WriteUnicodeString(item.Id.ToString());
            switch (item.GemSlot)
            {
                case GemSlot.PET:
                    pWriter.WriteInt(item.PetSkinBadgeId);
                    break;
                case GemSlot.TRANS:
                    pWriter.WriteBytes(item.TransparencyBadgeBools);
                    break;
            }
        }

        // Item Transfer Data 0x058AD00
        pWriter.WriteInt((int) item.TransferFlag);
        pWriter.WriteByte();
        pWriter.WriteInt(item.RemainingTrades);
        pWriter.WriteInt(1 - item.RemainingRepackageCount);
        pWriter.WriteByte();
        pWriter.WriteByte(); // 2nd flag, use to skip charbound

        // CharBound means untradable, unsellable, bound to char (ignores TransferFlag, but not 2nd flag!!)
        bool isCharBound = item.OwnerCharacterId != 0;
        pWriter.WriteBool(isCharBound);
        if (isCharBound)
        {
            pWriter.WriteLong(item.OwnerCharacterId);
            pWriter.WriteUnicodeString(item.OwnerCharacterName);
        }

        pWriter.WriteSockets(item.Stats);

        pWriter.WriteLong(item.PairedCharacterId);
        if (item.PairedCharacterId != 0)
        {
            pWriter.WriteUnicodeString(item.PairedCharacterName);
            pWriter.WriteBool(false);
        }

        // Unknwon | BoundCharacter?
        pWriter.WriteLong();
        pWriter.WriteUnicodeString();

        return pWriter;
    }

    private static PacketWriter WriteAppearance(this PacketWriter pWriter, Item item)
    {
        pWriter.Write(item.Color);
        // Positioning Data
        switch (item.ItemSlot)
        {
            case ItemSlot.CP:
                pWriter.Write(item.HatData);
                break;
            case ItemSlot.HR:
                pWriter.Write(item.HairData.BackLength);
                pWriter.Write(item.HairData.BackPositionCoord);
                pWriter.Write(item.HairData.BackPositionRotation);
                pWriter.Write(item.HairData.FrontLength);
                pWriter.Write(item.HairData.FrontPositionCoord);
                pWriter.Write(item.HairData.FrontPositionRotation);
                break;
            case ItemSlot.FD:
                pWriter.WriteBytes(item.FaceDecorationData);
                break;
        }

        return pWriter;
    }

    // 9 Blocks of stats, still missing some stats
    private static PacketWriter WriteStats(this PacketWriter pWriter, ItemStats stats)
    {
        pWriter.WriteByte(); // Not part of appearance sub!
        List<BasicStat> basicConstantNormalStats = stats.Constants.Values.OfType<BasicStat>().ToList();
        pWriter.WriteShort((short) basicConstantNormalStats.Count);
        foreach (BasicStat stat in basicConstantNormalStats)
        {
            WriteBasicStat(pWriter, stat);
        }

        List<SpecialStat> basicConstantSpecialStats = stats.Constants.Values.OfType<SpecialStat>().ToList();
        pWriter.WriteShort((short) basicConstantSpecialStats.Count);
        foreach (SpecialStat stat in basicConstantSpecialStats)
        {
            WriteSpecialStat(pWriter, stat);
        }
        pWriter.WriteInt();

        List<BasicStat> staticNormalStats = stats.Statics.Values.OfType<BasicStat>().ToList();
        pWriter.WriteShort((short) staticNormalStats.Count);
        foreach (BasicStat stat in staticNormalStats)
        {
            WriteBasicStat(pWriter, stat);
        }

        List<SpecialStat> staticSpecialStats = stats.Statics.Values.OfType<SpecialStat>().ToList();
        pWriter.WriteShort((short) staticSpecialStats.Count);
        foreach (SpecialStat stat in staticSpecialStats)
        {
            WriteSpecialStat(pWriter, stat);
        }

        pWriter.WriteInt();

        List<BasicStat> bonusNormalStats = stats.Randoms.Values.OfType<BasicStat>().ToList();
        pWriter.WriteShort((short) bonusNormalStats.Count);
        foreach (BasicStat stat in bonusNormalStats)
        {
            WriteBasicStat(pWriter, stat);
        }

        List<SpecialStat> bonusSpecialStats = stats.Randoms.Values.OfType<SpecialStat>().ToList();
        pWriter.WriteShort((short) bonusSpecialStats.Count);
        foreach (SpecialStat stat in bonusSpecialStats)
        {
            WriteSpecialStat(pWriter, stat);
        }
        pWriter.WriteInt();

        pWriter.WriteShort(); // Title Attributes
        pWriter.WriteShort();
        pWriter.WriteInt();
        pWriter.WriteShort(); // Empowerment Attributes
        pWriter.WriteShort();
        pWriter.WriteInt();
        pWriter.WriteShort(); // Empowerment Attributes
        pWriter.WriteShort();
        pWriter.WriteInt();
        pWriter.WriteShort(); // Empowerment Attributes
        pWriter.WriteShort();
        pWriter.WriteInt();
        pWriter.WriteShort(); // Empowerment Attributes
        pWriter.WriteShort();
        pWriter.WriteInt();
        pWriter.WriteShort(); // Empowerment Attributes
        pWriter.WriteShort();
        pWriter.WriteInt();

        return pWriter;
    }

    private static void WriteBasicStat(PacketWriter pWriter, BasicStat stat)
    {
        pWriter.WriteShort(stat.WriteAttribute());
        pWriter.WriteInt(stat.Flat);
        pWriter.WriteFloat(stat.Rate);
    }

    private static void WriteSpecialStat(PacketWriter pWriter, SpecialStat stat)
    {
        pWriter.WriteShort(stat.WriteAttribute());
        pWriter.WriteFloat(stat.Rate);
        pWriter.WriteFloat(stat.Flat);
    }

    private static PacketWriter WriteEnchantStats(this PacketWriter pWriter, Item item)
    {
        ItemStats stats = item.Stats;
        List<BasicStat> enchantStats = stats.Enchants.Values.OfType<BasicStat>().ToList();
        pWriter.WriteByte((byte) enchantStats.Count);
        foreach (BasicStat stat in enchantStats)
        {
            pWriter.WriteInt((int) stat.ItemAttribute);
            pWriter.WriteInt(stat.Flat);
            pWriter.WriteFloat(stat.Rate);
        }

        pWriter.WriteInt(item.LimitBreakLevel);

        List<BasicStat> basicLimitBreakStats = stats.LimitBreakEnchants.Values.OfType<BasicStat>().ToList();
        pWriter.WriteInt(basicLimitBreakStats.Count);
        foreach (BasicStat stat in basicLimitBreakStats)
        {
            WriteBasicStat(pWriter, stat);
        }

        List<SpecialStat> specialLimitBreakStats = stats.LimitBreakEnchants.Values.OfType<SpecialStat>().ToList();
        pWriter.WriteInt(specialLimitBreakStats.Count);
        foreach (SpecialStat stat in specialLimitBreakStats)
        {
            WriteSpecialStat(pWriter, stat);
        }

        return pWriter;
    }

    // Writes UGC template data
    private static void WriteTemplate(this PacketWriter pWriter, UGC ugc)
    {
        pWriter.WriteUGCTemplate(ugc);
        pWriter.WriteLong();
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteInt();
        pWriter.WriteLong();
        pWriter.WriteInt();
        pWriter.WriteLong();
        pWriter.WriteLong();
        pWriter.WriteUnicodeString();
    }

    private static PacketWriter WritePet(this PacketWriter pWriter)
    {
        pWriter.WriteUnicodeString(); // Name
        pWriter.WriteLong(); // Exp
        pWriter.WriteInt();
        pWriter.WriteInt(1); // Level
        pWriter.WriteByte();

        return pWriter;
    }

    public static PacketWriter WriteSockets(this PacketWriter pWriter, ItemStats stats)
    {
        pWriter.WriteByte((byte) stats.GemSockets.Count);
        int unlockedCount = 0;
        for (int i = 0; i < stats.GemSockets.Count; i++)
        {
            if (stats.GemSockets[i].IsUnlocked)
            {
                unlockedCount++;
            }
        }
        pWriter.WriteByte((byte) unlockedCount);
        for (int i = 0; i < unlockedCount; i++)
        {
            pWriter.WriteBool(stats.GemSockets[i].Gemstone != null);
            if (stats.GemSockets[i].Gemstone != null)
            {
                pWriter.WriteInt(stats.GemSockets[i].Gemstone.Id);
                pWriter.WriteBool(stats.GemSockets[i].Gemstone.OwnerId != 0);
                if (stats.GemSockets[i].Gemstone.OwnerId != 0)
                {
                    pWriter.WriteLong(stats.GemSockets[i].Gemstone.OwnerId);
                    pWriter.WriteUnicodeString(stats.GemSockets[i].Gemstone.OwnerName);
                }
                pWriter.WriteBool(stats.GemSockets[i].Gemstone.IsLocked);
                if (stats.GemSockets[i].Gemstone.IsLocked)
                {
                    pWriter.WriteBool(stats.GemSockets[i].Gemstone.IsLocked);
                    pWriter.WriteLong(stats.GemSockets[i].Gemstone.UnlockTime);
                }
            }
        }

        return pWriter;
    }

    private static PacketWriter WriteMusicScore(this PacketWriter pWriter, Item item)
    {
        pWriter.WriteInt(item.Score.Length);
        pWriter.WriteInt(item.Score.Type);
        pWriter.WriteUnicodeString(item.Score.Title);
        pWriter.WriteUnicodeString(item.Score.Composer);
        pWriter.WriteInt(4); // seems like it's always 4. 1 in KMS2. 
        pWriter.WriteLong(item.Score.ComposerCharacterId);
        pWriter.WriteBool(item.Score.Locked);
        pWriter.WriteLong();
        pWriter.WriteLong();
        return pWriter;
    }
}
