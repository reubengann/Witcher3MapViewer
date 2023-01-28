using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace SaveFile
{
    public class Witcher3SaveFile
    {
        public Witcher3CJournal CJournalManager;
        public Witcher3GwentManager GwentManager;
        public int CharacterLevel, CharacterFreeXP;

        static List<int> validFileVersions = new List<int> { 18, 19, 23 };

        int VariableTableOffset;
        int StringTableFooterOffset;
        int StringTableOffset;
        int NMSectionOffset;
        int RBSectionOffset;
        List<VariableIndexEntry> MainVariableIndex;
        List<VariableIndexEntry> VariableIndex;
        List<Witcher3GenericVariable> Hierarchy;
        List<string> StringTable;
        int position;
        ArrayStringReader arrayStringReader;
        List<string> SimplePorpHandleTypes = new List<string> {"W3LevelManager", "W3EffectManager", "CEncounterDataManager",
                          "CCharacterStats", "W3AbilityManager", "CBaseGameplayEffect",
                          "CPlayerInput", "WeaponHolster", "W3FactionReputationPoints",
                          "W3Reputation", "ISpawnTreeSpawnMonitorInitializer", "CSSPPL", "CInGameConfigWrapper" };
        List<string> SimpleSetTypes = new List<string> { "STutorialHighlight", "SUITutorial", "STutorialMessage" };
        List<string> SimpleSetsWithSizes = new List<string> { "ErrandDetailsList", "GameTime", "SMonsterNestUpdateDefinition",
            "SAbilityAttributeValue", "SBuffImmunity", "SItemUniqueId", "SGlossaryImageOverride",
            "SRadialSlotDef", "SLevelDefinition", "SSpendablePoints", "CreaturesGroupDef",
             "SBaseStat", "SGameplayFact", "SResistanceValue", "SSkill", "SSkillSlot",
            "SMutagenSlot", "SMutagenBonusAlchemy19", "SMutation", "SMutationProgress",
            "SInputActionLock", "CreatureCounterDef", "SOwnerEncounterTaskParams",
            "SExternalEncounterTaskParams", "SBuffPauseLock", "STemporarilyPausedEffect", "SWeatherBonus",
            "SLeaderBoardData", "SBlockedAbility", "SRewardMultiplier", "SSelectedQuickslotItem"};
        List<string> JustGrabACode = new List<string> {"CName", "ETutorialMessageType", "ETutorialHintDurationType", "ECompareOp",
                              "EZoneName", "EDifficultyMode", "EDoorState", "EFocusModeVisibility", "ESwitchState",
                              "EEffectType", "ESignType", "EVehicleSlot", "EHorseMode", "EBaseCharacterStats",
                              "EBaseCharacterStats", "ECharacterDefenseStats", "ESkill", "ESkillPath",
                              "ESkillSubPath", "EEquipmentSlots", "EPlayerMutationType", "ESkillColor",
                              "EIllusionDiscoveredOneliner", "EOperator", "EBehaviorGraph",
                              "EEncounterMonitorCounterType", "EDayPart", "EWeatherEffect", "EMoonState",
                              "W3TableState", "EFocusModeSoundEffectType", "EJournalStatus",
                              "EAIAttitude", "eGwintFaction", "EPlayerWeapon"};

        
        //public long TimeToLoadJournal, TimeToFindXP;

        public Witcher3SaveFile(string filename)
        {
            LoadSaveFromFile(filename);
        }

        public Witcher3SaveFile(string filename, Witcher3ReadLevel ReadLevel)
        {
            if (ReadLevel == Witcher3ReadLevel.Complete)
                LoadSaveFromFile(filename);

            else
            {
                LoadSaveQuick(filename);
            }
        }

        private void LoadSaveQuick(string filename)
        {
            //Load the CJournalManager, Gwent Manager, and levelManager only
            using (FileStream compressedStream = File.OpenRead(filename))
            using (Stream decompressedStream = ChunkedLz4File.Decompress(compressedStream))
            using (BinaryReader f = new BinaryReader(decompressedStream, System.Text.Encoding.GetEncoding("ISO-8859-1")))
            {
                ReadHeader(f);
                ReadFooter(f);
                ReadStringTable(f);
                ReadVariableTable(f);
                position = 0;
                Hierarchy = new List<Witcher3GenericVariable>();
                arrayStringReader = new ArrayStringReader();

                //Load the journal
                //VariableIndexEntry JournalIndex = ScanForMainVariableByName(f, "CJournalManager");
                //position = LocateVariableOffsetInIndex(JournalIndex);
                //Witcher3Collection member = ReadMember(f) as Witcher3Collection;
                //if (member.Type != Witcher3RawType.BS)
                //    throw new System.Exception("Top level variables are always BS!");
                //Hierarchy.Add(member);
                LoadTopLevelBS(f, "CJournalManager");
                LoadTopLevelBS(f, "CR4GwintManager");


                //Load the gwent manager
                //VariableIndexEntry GwentIndex = ScanForMainVariableByName(f, "CR4GwintManager");
                //position = LocateVariableOffsetInIndex(GwentIndex);
                //member = ReadMember(f) as Witcher3Collection;
                //if (member.Type != Witcher3RawType.BS)
                //    throw new System.Exception("Top level variables are always BS!");
                //Hierarchy.Add(member);

                // Load the levelManager (this could probably be improved by skipping entityTables, but
                // given that loading the journal takes about roughly the same amount of time,
                // the time savings are not that huge)
                VariableIndexEntry UniverseIndex = ScanForMainVariableByName(f, "universe");
                position = LocateVariableOffsetInIndex(UniverseIndex);
                f.Seek(VariableIndex[position].Offset);
                bool found = false;
                byte P = (byte)'P';
                while (f.BaseStream.Position != f.BaseStream.Length && !found)
                {
                    long foobar = f.BaseStream.Position;
                    if (f.ReadByte() == P)
                    {
                        f.SeekRelative(-1);
                        if (f.PeekIfString(2) && f.PeekString(2) == "PO")
                        {
                            int bytesread = 0;
                            if (f.PeekIfString(4) && f.PeekString(4) == "PORP")
                            {
                                f.ReadString(4);
                                bytesread += 4;
                                string Name = StringTable[f.ReadInt16()];
                                bytesread += 2;
                                string SpecificType = StringTable[f.ReadInt16()];
                                bytesread += 2;
                                if (Name == "levelManager")
                                {
                                    if (SpecificType == "handle:W3LevelManager")
                                    {
                                        f.SeekRelative(-bytesread);
                                        Witcher3PORP LevelManagerRaw = ReadVariable(f, VariableIndex[position].Size) as Witcher3PORP;
                                        Witcher3HandleData LevelData = LevelManagerRaw.Value as Witcher3HandleData;
                                        GetLevel(LevelData);
                                        found = true;
                                    }
                                }
                            }
                            else
                            {   //PO but not PORP, just a coincidence
                                f.SeekRelative(1);
                            }
                        }
                        else
                        {   //no PORP here, keep looking
                            f.SeekRelative(1);
                        }
                    }
                }
                //TimeToFindXP = stopwatch.ElapsedMilliseconds - temp;

                /*
                 * This way of finding the levelmanager somehow takes 4x longer than the above method.
                temp = stopwatch.ElapsedMilliseconds;
                f.Seek(VariableIndex[position].Offset);
                byte[] LevelManagerSequence = new byte[8];
                Encoding.ASCII.GetBytes("PORP").CopyTo(LevelManagerSequence, 0);
                byte[] aux = BitConverter.GetBytes((short)StringTable.IndexOf("levelManager"));                
                aux.CopyTo(LevelManagerSequence, 4);
                aux = BitConverter.GetBytes((short)StringTable.IndexOf("handle:W3LevelManager"));                
                aux.CopyTo(LevelManagerSequence, 6);
                int foo = (int)f.FindPosition(LevelManagerSequence);
                f.Seek(foo);
                Witcher3PORP LevelManagerRaw2 = ReadVariable(f, VariableIndex[position].Size) as Witcher3PORP;
                Witcher3HandleData LevelData2 = LevelManagerRaw2.Value as Witcher3HandleData;
                GetLevel(LevelData2);
                TimeToFindXP2 = stopwatch.ElapsedMilliseconds - temp;
                */
            }

            CJournalManager = new Witcher3CJournal(FindInHierarchy("CJournalManager"));
            GwentManager = new Witcher3GwentManager(FindInHierarchy("CR4GwintManager"));
        }

        private void LoadSaveFromFile(string filename)
        {
            using (FileStream compressedStream = File.OpenRead(filename))
            using (Stream decompressedStream = ChunkedLz4File.Decompress(compressedStream))
            using (BinaryReader f = new BinaryReader(decompressedStream, System.Text.Encoding.GetEncoding("ISO-8859-1")))
            {
                ReadHeader(f);
                ReadFooter(f);
                ReadStringTable(f);
                ReadVariableTable(f);
                position = 0;
                Hierarchy = new List<Witcher3GenericVariable>();
                arrayStringReader = new ArrayStringReader();
                while (position < VariableIndex.Count - 1)
                {
                    Witcher3Collection member = ReadMember(f) as Witcher3Collection;
                    if (member.Type != Witcher3RawType.BS)
                        throw new System.Exception("Top level variables are always BS!");

                    Hierarchy.Add(member);
                    position++;
                }
            }

            CJournalManager = new Witcher3CJournal(FindInHierarchy("CJournalManager"));
            Witcher3HandleData LevelData = FindLevelManager();
            GetLevel(LevelData);
        }

        private void LoadTopLevelBS(BinaryReader f, string name)
        {
            VariableIndexEntry Index = ScanForMainVariableByName(f, name);
            position = LocateVariableOffsetInIndex(Index);
            Witcher3Collection member = ReadMember(f) as Witcher3Collection;
            if (member.Type != Witcher3RawType.BS)
                throw new System.Exception("Top level variables are always BS!");
            Hierarchy.Add(member);
        }

        VariableIndexEntry ScanForMainVariableByName(BinaryReader f, string query)
        {
            foreach (VariableIndexEntry vie in MainVariableIndex)
            {
                f.Seek(vie.Offset);
                Witcher3BS member = ReadVariable(f, vie.Size) as Witcher3BS;
                if (member.Name == query)
                    return vie;
            }
            return null;
        }

        int LocateVariableOffsetInIndex(VariableIndexEntry query)
        {
            int i = 0;
            while (i < VariableIndex.Count)
            {
                if (VariableIndex[i].Offset == query.Offset)
                    return i;
                i++;
            }
            return -1;
        }

        /*
         * BS variables can live inside other BS variables, but eventually you get down to something atomic.
         * We need a function that can handle both cases. This function doesn't really care what type something
         * is, it adds to the tree until the number of bytes is satisfied, regardless of type.
         */
        Witcher3GenericVariable ReadMember(BinaryReader f)
        {
            VariableIndexEntry CurrentIndex = VariableIndex[position];
            f.Seek(CurrentIndex.Offset);
            int remaining = CurrentIndex.Size;
            Witcher3GenericVariable readmember = ReadVariable(f, remaining);
            remaining -= readmember.Size;
            if (!(readmember is Witcher3Collection))
            {
                if (remaining != 0)
                    throw new System.Exception("Wrong number of bytes read");

                return readmember;
            }
            Witcher3Collection coll = readmember as Witcher3Collection;
            while (position < VariableIndex.Count && remaining >= VariableIndex[position + 1].Size)
            {
                position++;
                Witcher3GenericVariable NewMember = ReadMember(f);
                remaining -= NewMember.Size;
                coll.Members.Add(NewMember);
                coll.Size += NewMember.Size;
            }
            if (remaining != 0)
                throw new System.Exception("Wrong number of bytes read");

            return coll;
        }


        /*
         * ReadVariable is what determines the variable's identity. It doesn't hierarchize
         * unless it is explicitly told to (such as an SS)             
        */
        Witcher3GenericVariable ReadVariable(BinaryReader f, int readatmost)
        {
            if (!f.PeekIfString(2))
                throw new System.Exception("No type declaration while reading variable");
            string FirstTwoLetters = f.PeekString(2);
            if (FirstTwoLetters == "BS")
                return ReadBSVariable(f);
            if (FirstTwoLetters == "VL")
                return ReadVLVariable(f);
            if (FirstTwoLetters == "SX")
                return ReadSXAPVariable(f, readatmost);
            if (FirstTwoLetters == "SB")
                return ReadSBDFVariable(f, readatmost);
            if (FirstTwoLetters == "RO")
                return ReadWitcher3ROTSVariable(f);
            if (FirstTwoLetters == "SS")
                return ReadWitcher3SSVariable(f);
            if (FirstTwoLetters == "OP")
                return ReadWitcher3OPVariable(f);
            if (FirstTwoLetters == "BL")
                return ReadWitcher3BLCKVariable(f);
            if (FirstTwoLetters == "AV")
                return ReadWitcher3AVALVariable(f);
            if (FirstTwoLetters == "PO")
                return ReadWitcher3PORPVariable(f);
            throw new System.Exception("Internal type " + FirstTwoLetters + " not implemented");
        }

        Witcher3PORP ReadWitcher3PORPVariable(BinaryReader f)
        {
            int bytesread = 0;
            if (f.ReadString(4) != "PORP")
                throw new System.Exception("Invalid PORP");
            bytesread += 4;
            Witcher3PORP variable = new Witcher3PORP();
            variable.Name = StringTable[f.ReadInt16()];
            bytesread += 2;
            variable.SpecificType = StringTable[f.ReadInt16()];
            bytesread += 2;
            int porpsize = f.ReadInt16();
            bytesread += 2;
            if (f.ReadInt16() != 0)
                throw new System.Exception("Expected zero in porp variable"); //may actually be an Int32
            bytesread += 2;
            if (variable.SpecificType.StartsWith("handle:"))
            {
                Witcher3HandleData temp = ReadPorpHandle(f, variable.SpecificType);
                if (temp.Size != porpsize)
                    throw new System.Exception("Wrong number of bytes in PORP");
                bytesread += temp.Size;
                variable.Value = temp;
            }
            else
            {
                Witcher3Value temp = ReadValue(f, variable.SpecificType, true);
                if (temp.Size != porpsize)
                    throw new System.Exception("Wrong number of bytes in PORP");
                bytesread += temp.Size;
                variable.Value = temp;
            }
            variable.Size = bytesread;
            return variable;
        }

        Witcher3AVAL ReadWitcher3AVALVariable(BinaryReader f)
        {
            int bytesread = 0;
            if (f.ReadString(4) != "AVAL")
                throw new System.Exception("Invalid AVAL");
            bytesread += 4;
            Witcher3AVAL variable = new Witcher3AVAL();
            variable.Name = StringTable[f.ReadInt16()];
            bytesread += 2;
            variable.SpecificType = StringTable[f.ReadInt16()];
            bytesread += 2;
            int AVALBytes = f.ReadInt32();
            bytesread += 4;
            Witcher3Value temp = ReadValue(f, variable.SpecificType, true);
            if (temp.Size != AVALBytes)
                throw new System.Exception("Wrong number of bytes in AVAL");
            bytesread += temp.Size;
            variable.Value = temp;
            variable.Size = bytesread;
            return variable;
        }

        Witcher3BLCK ReadWitcher3BLCKVariable(BinaryReader f)
        {
            int bytesread = 0;
            if (f.ReadString(4) != "BLCK")
                throw new System.Exception("Invalid BLCK");
            bytesread += 4;
            Witcher3BLCK variable = new Witcher3BLCK();
            variable.Name = StringTable[f.ReadInt16()];
            bytesread += 2;
            int blockSize = f.ReadInt32();
            bytesread += 4;
            int toread = blockSize;
            while (toread > 0)
            {
                Witcher3GenericVariable temp = ReadVariable(f, toread);
                bytesread += temp.Size;
                toread -= temp.Size;
                variable.Members.Add(temp);
            }
            if (toread != 0)
                throw new System.Exception("Too many bytes read in BLCK");
            variable.Size = bytesread;
            return variable;
        }

        Witcher3OP ReadWitcher3OPVariable(BinaryReader f)
        {
            if (f.ReadString(2) != "OP")
                throw new System.Exception("Invalid OP");
            int bytesread = 2;
            Witcher3OP variable = new Witcher3OP();
            variable.Name = StringTable[f.ReadInt16()];
            bytesread += 2;
            variable.SpecificType = StringTable[f.ReadInt16()];
            bytesread += 2;
            Witcher3Value val = ReadValue(f, variable.SpecificType, false);
            bytesread += val.Size;
            variable.Value = val;
            variable.Size = bytesread;
            return variable;
        }

        Witcher3SS ReadWitcher3SSVariable(BinaryReader f)
        {
            int bytesread = 0;
            if (f.ReadString(2) != "SS")
                throw new System.Exception("Invalid SS");
            bytesread += 2;
            Witcher3SS variable = new Witcher3SS();
            int toread = f.ReadInt32();
            bytesread += 4;
            while (toread > 0)
            {
                Witcher3GenericVariable temp = ReadVariable(f, toread);
                bytesread += temp.Size;
                toread -= temp.Size;
                variable.Members.Add(temp);
            }
            if (toread != 0)
                throw new System.Exception("Wrong number of bytes read in SS");
            variable.Size = bytesread;
            return variable;
        }


        Witcher3ROTS ReadWitcher3ROTSVariable(BinaryReader f)
        {
            int bytesread = 0;
            Witcher3ROTS variable = new Witcher3ROTS();
            if (f.ReadString(4) != "ROTS")
                throw new System.Exception("Unknown RO magic string");
            bytesread += 4;
            int toread = f.ReadInt32();
            bytesread += 4;
            while (toread > 0)
            {
                Witcher3GenericVariable temp = ReadVariable(f, toread);
                bytesread += temp.Size;
                toread -= temp.Size;
                variable.Members.Add(temp);
            }
            if (toread != 0)
                throw new System.Exception("Wrong number of bytes read in STOR");
            if (f.ReadString(4) != "STOR")
                throw new System.Exception("Wrong ending in STOR");
            bytesread += 4;
            variable.Size = bytesread;
            return variable;
        }

        //TODO: try to understand this
        Witcher3SBDF ReadSBDFVariable(BinaryReader f, int readatmost)
        {
            Witcher3SBDF variable = new Witcher3SBDF();
            if (f.ReadString(4) != "SBDF")
                throw new System.Exception("Unknown SB magic string");
            int bytesread = 4;
            f.SeekRelative(readatmost - bytesread - 4);
            if (f.ReadString(4) != "EBDF")
                throw new System.Exception("SBDF termination missing!");
            bytesread = readatmost;
            variable.Size = bytesread;
            return variable;
        }

        Witcher3SXAP ReadSXAPVariable(BinaryReader f, int readatmost)
        {
            Witcher3SXAP variable = new Witcher3SXAP();
            if (f.ReadString(4) != "SXAP")
                throw new System.Exception("Unknown SX magic string");
            int bytesread = 4;
            VerifyTypecodes(f);
            bytesread += 12;
            if (f.PeekIfString(2) && !f.PeekIfString(4))
                throw new System.Exception("Two-letter var in SXAP body!");
            if (f.PeekIfString(4)) //SXAP contains named variables (BLCK, PORP, and AVAL)
            {
                while (bytesread < readatmost)
                {
                    Witcher3GenericVariable temp = ReadVariable(f, readatmost - bytesread);
                    bytesread += temp.Size;
                    variable.Members.Add(temp);
                    if (!f.PeekIfString(2))
                    {
                        //Give up
                        f.SeekRelative(readatmost - bytesread);
                        bytesread = readatmost;
                    }
                }
            }
            else //TODO: make sense of this data
            {
                variable.Hidden = true;
                int count = f.ReadInt32();
                bytesread += 4;
                f.SeekRelative(readatmost - bytesread);
                bytesread = readatmost;
            }
            if (bytesread != readatmost)
                throw new System.Exception("Wrong amount of data in SXAP");
            variable.Size = bytesread;
            return variable;
        }

        Witcher3VL ReadVLVariable(BinaryReader f)
        {
            if (f.ReadString(2) != "VL")
                throw new System.Exception("Unknown VL magic string");
            int bytesread = 2;
            int startoffset = f.Tell() - 2;
            Witcher3VL variable = new Witcher3VL();
            variable.Name = StringTable[f.ReadInt16()];
            bytesread += 2;
            variable.SpecificType = StringTable[f.ReadInt16()];
            bytesread += 2;
            Witcher3Value Value = ReadValue(f, variable.SpecificType, false);
            bytesread += Value.Size;
            variable.Value = Value;
            variable.Offset = startoffset;
            variable.Size = bytesread;
            return variable;
        }

        Witcher3BS ReadBSVariable(BinaryReader f)
        {
            if (f.ReadString(2) != "BS")
                throw new System.Exception("Unknown BS magic string");
            int bytesread = 2;
            int startoffset = f.Tell() - 2;
            Witcher3BS variable = new Witcher3BS();
            variable.Name = StringTable[f.ReadUInt16()];
            bytesread += 2;
            variable.Size = bytesread;
            variable.Offset = startoffset;
            return variable;
        }

        Witcher3Value ReadValue(BinaryReader f, string TypeName, bool sizeCheck)
        {
            if (TypeName.StartsWith("array:"))
                return ReadArray(f, TypeName, sizeCheck);
            if (TypeName.StartsWith("handle:"))
            {
                if (sizeCheck)
                    return ReadPorpHandle(f, TypeName);
                else
                    return ReadRegularHandle(f, TypeName);
            }
            if (TypeName.StartsWith("soft:"))
            {
                string Type = TypeName.Substring("soft:".Length);
                return ReadValue(f, Type, sizeCheck);
            }
            if (TypeName == "CGUID")
                return ReadGUID(f);
            if (TypeName == "IdTag")
            {
                f.ReadByte(); //can be 0 or 1, but doesn't seem to matter
                int bytesread = 1;
                Witcher3Value val = ReadGUID(f);
                val.Type = "IdTag";
                val.Size += bytesread;
                return val;
            }
            if (TypeName == "String" || TypeName == "CEntityTemplate")
                return ReadListpackString(f);
            if (TypeName == "StringAnsi")
            {
                int L = f.ReadByte();
                string s = f.ReadString(L - 1);
                if (f.ReadByte() != 0)
                    throw new System.Exception("no zero termination in Ansi String");
                return new Witcher3Value(s, L + 1, TypeName);
            }
            if (TypeName == "SQuestThreadSuspensionData")
                return ReadSQuestThreadSuspensionData(f);
            if (TypeName == "SActionPointId")
                return ReadSActionPointID(f);
            if (TypeName == "EntityHandle")
                return ReadEntityHandle(f);
            if (TypeName == "Uint8")
                return new Witcher3Value(f.ReadByte(), 1, TypeName);
            if (TypeName == "Uint16")
                return new Witcher3Value(f.ReadUInt16(), 2, TypeName);
            if (TypeName == "Int16")
                return new Witcher3Value(f.ReadInt16(), 2, TypeName);
            if (TypeName == "Uint32")
                return new Witcher3Value(f.ReadUInt32(), 4, TypeName);
            if (TypeName == "Int32")
                return new Witcher3Value(f.ReadInt32(), 4, TypeName);
            if (TypeName == "Uint64")
                return new Witcher3Value(f.ReadUInt64(), 8, TypeName);
            if (TypeName == "LocalizedString") //Does this ever appear?
                return new Witcher3Value(f.ReadUInt32(), 4, TypeName);
            if (TypeName == "Float")
                return new Witcher3Value(f.ReadFloat(), 4, TypeName);
            if (TypeName == "Double")
                return new Witcher3Value(f.ReadDouble(), 8, TypeName);
            if (TypeName == "Bool")
                return new Witcher3Value(f.ReadByte() == 1, 1, TypeName);
            if (TypeName == "Vector")
                return ReadVector(f, sizeCheck);
            if (TypeName == "Vector3")
                return ReadVector3(f, sizeCheck);
            if (TypeName == "Vector2")
                return ReadVector2(f, sizeCheck);
            if (TypeName == "EulerAngles")
                return ReadEulerAngles(f, sizeCheck);
            if (TypeName == "Matrix")
                return ReadMatrix(f, sizeCheck);
            if (TypeName == "EngineTime")
                return ReadEngineTime(f);
            if (TypeName == "EngineTransform")
                return ReadEngineTransform(f);
            if (SimpleSetTypes.Contains(TypeName))
            {
                Witcher3SimpleSet set = ReadSimpleSetVal(f);
                set.Type = TypeName;
                return set;
            }
            if (TypeName == "TagList")
                return ReadTagList(f);
            if (SimpleSetsWithSizes.Contains(TypeName))
            {
                Witcher3SimpleSet set = ReadSimpleSetWithSizes(f, sizeCheck);
                set.Type = TypeName;
                return set;
            }
            if (TypeName == "GameTimeWrapper") //is this ever called with sizeCheck off?
                return ReadGameTimeWrapper(f);
            if (JustGrabACode.Contains(TypeName))
                return new Witcher3Value(StringTable[f.ReadUInt16()], 2, TypeName);
            throw new System.Exception("Type " + TypeName + " not handle");
        }

        Witcher3SimpleSet ReadGameTimeWrapper(BinaryReader f)
        {
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("No zero byte in GameTimeWrapper");
            bytesread++;
            Witcher3Value val = ReadSingleSizeSpec(f);
            if (val.Type != "GameTime")
                throw new System.Exception("Expected a gametime");
            bytesread += val.Size;
            if (f.ReadUInt16() != 0)
                throw new System.Exception("Expected end of block");
            bytesread += 2;
            val.Size = bytesread;
            return val as Witcher3SimpleSet;
        }

        Witcher3SimpleSet ReadSimpleSetWithSizes(BinaryReader f, bool sizeCheck)
        {
            Witcher3SimpleSet val = new Witcher3SimpleSet();
            val.Type = "SimpleSet";
            int bytesread = 0;

            if (f.ReadByte() != 0)
                throw new System.Exception("No zero byte in simple set");
            bytesread += 1;
            bool Done = false;
            while (!Done)
            {
                if (f.PeekInt16() == 0)
                {
                    f.SeekRelative(2);
                    bytesread += 2;
                    Done = true;
                }
                else
                {
                    Witcher3Value temp;
                    if (sizeCheck) temp = ReadSingleSizeSpec(f);
                    else temp = ReadSingle(f);
                    bytesread += temp.Size;
                    val.SetInfo.Add(temp);
                }
            }
            val.Size = bytesread;
            return val;
        }

        TagList ReadTagList(BinaryReader f)
        {
            TagList val = new TagList();
            val.Type = "TagList";
            int bytesread = 0;
            int count = f.ReadByte() & 127; //ignore first byte
            bytesread++;
            for (int i = 0; i < count; i++)
            {
                val.Tags.Add(StringTable[f.ReadUInt16()]);
                bytesread += 2;
            }
            val.Size = bytesread;
            return val;
        }

        /*
         * EngineTransform is a total mystery to me. It seems to always come at the end of a 
         * ROTS/STOR block, but sometimes there are three trailing zero bytes that must be there,
         * and other times not. Thus, I can't just scan until 3 bytes before "STOR", because
         * sometimes they aren't there. The only thing that seems to determine the number of bytes
         * is the first byte, which can be 1, 2, 3, or 7 from what I've seen. But there's no real
         * pattern to the data. It might be an unknown enum.
        */
        EngineTransform ReadEngineTransform(BinaryReader f)
        {
            int bytesread = 0;
            EngineTransform val = new EngineTransform();
            val.Type = "EngineTransform";
            int numsets = f.ReadByte();
            bytesread++;
            if (numsets == 1 || numsets == 2)
            {
                for (int i = 0; i < 6; i++)
                {
                    f.ReadUInt16();
                    bytesread += 2;
                }
            }
            else if (numsets == 3)
            {
                for (int i = 0; i < 12; i++)
                {
                    f.ReadUInt16();
                    bytesread += 2;
                }
            }
            else if (numsets == 7)
            {
                for (int i = 0; i < 18; i++)
                {
                    f.ReadUInt16();
                    bytesread += 2;
                }
            }
            else
                throw new System.Exception("expected 1, 2, 3, or 7 in EngineTransform");
            val.Size = bytesread;
            return val;
        }

        EngineTime ReadEngineTime(BinaryReader f)
        {
            EngineTime val = new EngineTime();
            val.Type = "EngineTime";
            int bytesread = 0;
            val.timeval = f.ReadUInt16(); //is this always zero????
            bytesread += 2;
            if (f.ReadByte() != 0)
                throw new System.Exception("nonzero EngineTime zero byte");
            bytesread++;
            val.Size = bytesread;
            return val;
        }

        Witcher3SimpleSet ReadSimpleSetVal(BinaryReader f)
        {
            Witcher3SimpleSet val = new Witcher3SimpleSet();
            val.Type = "Simple Set";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("No zero byte in simple set");
            bytesread += 1;
            bool Done = false;
            while (!Done)
            {
                if (f.PeekInt16() == 0)
                {
                    f.SeekRelative(2);
                    bytesread += 2;
                    Done = true;
                }
                else
                {
                    Witcher3Value temp = ReadSingle(f);
                    bytesread += temp.Size;
                    val.SetInfo.Add(temp);
                }
            }
            val.Size = bytesread;
            return val;
        }

        Witcher3Matrix ReadMatrix(BinaryReader f, bool sizeCheck)
        {
            Witcher3Matrix val = new Witcher3Matrix();
            val.Type = "Matrix";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("No zero byte to start matrix");
            bytesread++;
            int found = 0;
            for (int i = 0; i < 4; i++)
            {
                Witcher3Value temp;
                if (sizeCheck)
                    temp = ReadSingleSizeSpec(f);
                else temp = ReadSingle(f);
                if (temp.Type != "Vector")
                    throw new System.Exception("Not a vector?");
                bytesread += temp.Size;
                switch (temp.Name)
                {
                    case "X":
                        val.X = temp as Witcher3Vector;
                        found++;
                        break;
                    case "Y":
                        val.Y = temp as Witcher3Vector;
                        found++;
                        break;
                    case "Z":
                        val.Z = temp as Witcher3Vector;
                        found++;
                        break;
                    case "W":
                        val.W = temp as Witcher3Vector;
                        found++;
                        break;
                    default:
                        break;
                }
            }
            if (found < 4)
                throw new System.Exception("Did not find all entries in Matrix");
            if (f.ReadUInt16() != 0)
                throw new System.Exception("expected 00 00 to end matrix");
            bytesread += 2;
            val.Size = bytesread;
            return val;
        }

        Witcher3EulerAngles ReadEulerAngles(BinaryReader f, bool sizeCheck)
        {
            Witcher3EulerAngles val = new Witcher3EulerAngles();
            val.Type = "EulerAngles";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("First byte of EulerAngles is not zero");
            bytesread++;
            int found = 0;
            for (int i = 0; i < 3; i++)
            {
                Witcher3Value temp;
                if (sizeCheck)
                {
                    temp = ReadSingleSizeSpec(f);
                    bytesread += temp.Size;
                }
                else
                {
                    temp = ReadSingle(f);
                    bytesread += 8;
                }
                if (temp.Type != "Float")
                    throw new System.Exception("EulerAngles value not a float");
                switch (temp.Name)
                {
                    case "Pitch":
                        val.Pitch = temp.Value as float?;
                        found++;
                        break;
                    case "Yaw":
                        val.Yaw = temp.Value as float?;
                        found++;
                        break;
                    case "Roll":
                        val.Roll = temp.Value as float?;
                        found++;
                        break;
                    default:
                        break;
                }

            }
            if (found < 3)
                throw new System.Exception("Did not find all entries in EulerAngles");
            if (f.ReadUInt16() != 0)
                throw new System.Exception("No null termination in EulerAngles");
            bytesread += 2;
            val.Size = bytesread;
            return val;
        }

        Witcher3Vector2 ReadVector2(BinaryReader f, bool sizeCheck)
        {
            Witcher3Vector2 val = new Witcher3Vector2();
            val.Type = "Vector2";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("First byte of vector2 is not zero");
            bytesread++;
            int found = 0;
            for (int i = 0; i < 2; i++)
            {
                Witcher3Value temp;
                if (sizeCheck)
                {
                    temp = ReadSingleSizeSpec(f);
                    bytesread += temp.Size;
                }
                else
                {
                    temp = ReadSingle(f);
                    bytesread += 8;
                }
                if (temp.Type != "Float")
                    throw new System.Exception("Vector2 value not a float");
                switch (temp.Name)
                {
                    case "X":
                        val.X = temp.Value as float?;
                        found++;
                        break;
                    case "Y":
                        val.Y = temp.Value as float?;
                        found++;
                        break;
                    default:
                        break;
                }

            }
            if (found < 2)
                throw new System.Exception("Did not find all entries in Vector2");
            if (f.ReadUInt16() != 0)
                throw new System.Exception("No null termination in Vector2");
            bytesread += 2;
            val.Size = bytesread;
            return val;
        }

        Witcher3Vector3 ReadVector3(BinaryReader f, bool sizeCheck)
        {
            Witcher3Vector3 val = new Witcher3Vector3();
            val.Type = "Vector3";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("First byte of vector3 is not zero");
            bytesread++;
            int found = 0;
            for (int i = 0; i < 3; i++)
            {
                Witcher3Value temp;
                if (sizeCheck)
                {
                    temp = ReadSingleSizeSpec(f);
                    bytesread += temp.Size;
                }
                else
                {
                    temp = ReadSingle(f);
                    bytesread += 8;
                }
                if (temp.Type != "Float")
                    throw new System.Exception("Vector3 value not a float");
                switch (temp.Name)
                {
                    case "X":
                        val.X = temp.Value as float?;
                        found++;
                        break;
                    case "Y":
                        val.Y = temp.Value as float?;
                        found++;
                        break;
                    case "Z":
                        val.Z = temp.Value as float?;
                        found++;
                        break;
                    default:
                        break;
                }

            }
            if (found < 3)
                throw new System.Exception("Did not find all entries in Vector3");
            if (f.ReadUInt16() != 0)
                throw new System.Exception("No null termination in Vector3");
            bytesread += 2;
            val.Size = bytesread;
            return val;
        }

        Witcher3Vector ReadVector(BinaryReader f, bool sizeCheck)
        {
            Witcher3Vector val = new Witcher3Vector();
            val.Type = "Vector";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("First byte of vector is not zero");
            bytesread++;
            int found = 0;
            for (int i = 0; i < 4; i++)
            {
                Witcher3Value temp;
                if (sizeCheck)
                {
                    temp = ReadSingleSizeSpec(f);
                    bytesread += temp.Size;
                }
                else
                {
                    temp = ReadSingle(f);
                    bytesread += 8;
                }
                if (temp.Type != "Float")
                    throw new System.Exception("Vector value not a float");
                switch (temp.Name)
                {
                    case "X":
                        val.X = temp.Value as float?;
                        found++;
                        break;
                    case "Y":
                        val.Y = temp.Value as float?;
                        found++;
                        break;
                    case "Z":
                        val.Z = temp.Value as float?;
                        found++;
                        break;
                    case "W":
                        val.W = temp.Value as float?;
                        found++;
                        break;
                    default:
                        break;
                }

            }
            if (found < 4)
                throw new System.Exception("Did not find all entries in Vector");
            if (f.ReadUInt16() != 0)
                throw new System.Exception("No null termination in Vector");
            bytesread += 2;
            val.Size = bytesread;
            return val;
        }

        EntityHandle ReadEntityHandle(BinaryReader f)
        {
            EntityHandle val = new EntityHandle();
            val.Type = "EntityHandle";
            int unknown1 = f.ReadByte();
            int bytesread = 1;
            val.unknown1 = unknown1;
            if (unknown1 == 0 || unknown1 == 3)
            {
                val.GUID = "unknown";
                val.Size = bytesread;
                return val;
            }
            if (unknown1 != 2)
                throw new System.Exception("Expected 2 in entityhandle");
            int unknown2 = f.ReadByte();
            bytesread += 1;
            if (unknown2 != 0 && unknown2 != 1)
                throw new System.Exception("Expected 0 or 1");
            val.GUID = ReadGUID(f).Value as string;
            bytesread += 16;
            val.Size = bytesread;
            return val;
        }

        SActionPointIdData ReadSActionPointID(BinaryReader f)
        {
            SActionPointIdData val = new SActionPointIdData();
            val.Type = "SActionPointIdData";
            int bytesread = 0;
            if (f.ReadByte() != 0)
                throw new System.Exception("nonzero SActionPointId zero bit needs analysis");
            bytesread += 1;
            int IdAddress = f.ReadUInt16();
            bytesread += 2;
            if (IdAddress <= 0)
            {
                val.Size = bytesread;
                return val;
            }
            val.IdName = StringTable[IdAddress];
            string Type = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (Type != "CGUID")
                throw new System.Exception("Non-CGUID data in SActionPointId spec");
            val.componentGUID = ReadGUID(f).Value as string;
            bytesread += 16;
            position++;
            val.entityName = StringTable[f.ReadUInt16()];
            bytesread += 2;
            Type = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (Type != "CGUID")
                throw new System.Exception("Non-CGUID data in SActionPointId spec");
            val.entityGUID = ReadGUID(f).Value as string;
            bytesread += 16;
            position++;
            if (f.ReadUInt16() != 0)
                throw new System.Exception("no zero termination in SQuestThreadSuspensionData");
            bytesread += 2;
            val.Size = bytesread;
            return val;
        }

        SQuestThreadSuspensionData ReadSQuestThreadSuspensionData(BinaryReader f)
        {
            SQuestThreadSuspensionData val = new SQuestThreadSuspensionData();
            val.Type = "SQuestThreadSuspensionData";
            if (f.ReadByte() != 0)
                throw new System.Exception("No zero byte in SQuestThreadSuspensionData");
            int bytesread = 1;
            string SQTrName = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (SQTrName != "scopeBlockGUID")
                throw new System.Exception("Failed to read SQuestThreadSuspensionData");
            string SQTrType = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (SQTrType != "CGUID")
                throw new System.Exception("No CGUID in SQuestThreadSuspensionData");
            val.scopeBlockGUID = ReadGUID(f).Value as string;
            position++; //GUID is held in the variable table
            bytesread += 16;
            string ArrayName = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (ArrayName != "scopeData")
                throw new System.Exception("Failed to read SQuestThreadSuspensionData");
            string ArrayType = StringTable[f.ReadUInt16()];
            bytesread += 2;
            Witcher3Value SQArray = ReadArray(f, ArrayType, false);
            position++; //array is held in the variable table
            bytesread += SQArray.Size;
            if (f.ReadUInt16() != 0)
                throw new System.Exception("No zero termination in SQuestThreadSuspensionData");
            bytesread += 2;
            val.QuestThread = SQArray;
            val.Size = bytesread;
            return val;
        }


        Witcher3Value ReadListpackString(BinaryReader f)
        {
            /* 
            * This follows the Listpack tiny string and multibyte specification. I.e.:
            * If the first two bits are 10xxxxxx (128 + xxxxxx) then read xxxxxx bytes
            * If both of them are set 11xxxxxx then also read the next byte to get the size
            * In either case we don't keep those two bits
            */
            Witcher3Value val = new Witcher3Value();
            val.Type = "String";
            int L = f.ReadByte();
            int bytesread = 1;
            if (L < 128)
                throw new System.Exception("Incorrect string specification");
            L = L - 128; //ignore first bit
            if (L == 0)
            {
                val.Value = "";
                val.Size = bytesread;
                return val;
            }
            bytesread += L;
            if (L >= 64) //second bit is set
            {
                L = L - 64; //ignore second bit
                L = f.ReadByte() * 64 + L;
                bytesread = L + 2;
            }
            val.Value = f.ReadString(L);
            val.Size = bytesread;
            return val;
        }

        /* The built-in GUID reader in .NET does not match up with CDProject's method */
        Witcher3Value ReadGUID(BinaryReader f)
        {
            int bytesread = 0;
            string guid = "";
            for (int i = 0; i < 4; i++)
            {
                string segment = "";
                for (int j = 0; j < 4; j++)
                {
                    byte b = f.ReadByte();
                    bytesread += 1;
                    segment = b.ToString("X2") + segment; //note reverse byte order
                }
                guid += segment;
                if (i < 3)
                    guid += "-";
            }
            Witcher3Value val = new Witcher3Value(guid, bytesread);
            val.Type = "CGUID";
            val.Size = bytesread;
            return val;
        }

        Witcher3HandleData ReadRegularHandle(BinaryReader f, string HandleString)
        {
            if (!HandleString.StartsWith("handle:"))
                throw new System.Exception("not a valid handle");
            string HandleType = HandleString.Substring("handle:".Length);
            byte StopReading = f.ReadByte();
            int bytesread = 1;
            if (StopReading != 0 && StopReading != 1)
                throw new System.Exception("Expected 0 or 1");
            if (StopReading == 1)
            {
                Witcher3HandleData empty = new Witcher3HandleData();
                empty.Size = bytesread;
                return empty;
            }
            if (HandleType != "W3TutorialManagerUIHandler" && HandleType != "W3EnvironmentManager")
            {
                throw new System.Exception("ReadRegularHandle does not currently handle type " + HandleType);
            }
            Witcher3HandleData handleData = new Witcher3HandleData();
            byte SpecifyTarget = f.ReadByte();
            bytesread += 1;
            handleData.unknowncode1 = f.ReadInt16();
            bytesread += 2;
            if (f.ReadUInt16() != 0)
                throw new System.Exception("expected 0");
            bytesread += 2;
            if (SpecifyTarget == 0)
            {
                handleData.Size = bytesread;
                return handleData;
            }
            handleData.specificType = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (f.ReadByte() != 0)
                throw new System.Exception("expected 0");
            bytesread += 1;
            bool Done = false;
            while (!Done)
            {
                if (f.PeekInt16() == 0)
                {
                    f.SeekRelative(2);
                    bytesread += 2;
                    Done = true;
                }
                else
                {
                    Witcher3Value val = ReadSingle(f);
                    bytesread += val.Size;
                    handleData.HandleInfo.Add(val);
                }
            }
            handleData.Size = bytesread;
            return handleData;
        }

        Witcher3HandleData ReadPorpHandle(BinaryReader f, string HandleString)
        {
            if (!HandleString.StartsWith("handle:"))
                throw new System.Exception("not a valid handle");
            string HandleType = HandleString.Substring("handle:".Length);
            byte StopReading = f.ReadByte();
            int bytesread = 1;
            if (StopReading != 0 && StopReading != 1)
                throw new System.Exception("Expected 0 or 1");
            if (StopReading == 1)
            {
                Witcher3HandleData empty = new Witcher3HandleData();
                empty.Size = bytesread;
                return empty;
            }
            if (!SimplePorpHandleTypes.Contains(HandleType))
            {
                throw new System.Exception("ReadPorpHandle does not currently handle type " + HandleType);
            }
            Witcher3HandleData handleData = new Witcher3HandleData();
            byte SpecifyTarget = f.ReadByte();
            bytesread += 1;
            handleData.unknowncode1 = f.ReadInt16();
            bytesread += 2;
            if (f.ReadUInt16() != 0)
                throw new System.Exception("expected 0");
            bytesread += 2;
            if (SpecifyTarget == 0)
            {
                handleData.Size = bytesread;
                return handleData;
            }
            handleData.specificType = StringTable[f.ReadUInt16()];
            bytesread += 2;
            if (f.ReadByte() != 0)
                throw new System.Exception("expected 0");
            bytesread += 1;
            bool Done = false;
            while (!Done)
            {
                if (f.PeekInt16() == 0)
                {
                    f.SeekRelative(2);
                    bytesread += 2;
                    Done = true;
                }
                else
                {
                    Witcher3Value val = ReadSingleSizeSpec(f);
                    bytesread += val.Size;
                    handleData.HandleInfo.Add(val);
                }
            }
            handleData.Size = bytesread;
            return handleData;
        }

        Witcher3Value ReadSingleSizeSpec(BinaryReader f)
        {
            int startoffset = f.Tell();
            int NameCode = f.ReadInt16();
            int bytesread = 2;
            if (NameCode == 0)
                throw new System.Exception("Null in ReadSingleSizeSpec");
            if (NameCode > StringTable.Count)
                throw new System.Exception("Name code out of bounds");
            string Type = StringTable[f.ReadInt16()];
            bytesread += 2;
            int toread = f.ReadInt16() - 4;
            bytesread += 2;
            if (f.ReadInt16() != 0)
                throw new System.Exception("Not zero");
            bytesread += 2;
            Witcher3Value val = ReadValue(f, Type, true);
            if (val.Size != toread)
                throw new System.Exception("wrong number of bytes read");
            val.Name = StringTable[NameCode];
            val.Size += bytesread;
            return val;
        }

        Witcher3Value ReadSingle(BinaryReader f)
        {
            int NameCode = f.ReadInt16();
            int bytesread = 2;
            if (NameCode == 0)
                throw new System.Exception("Null in ReadSingleSizeSpec");
            if (NameCode > StringTable.Count)
                throw new System.Exception("Name code out of bounds");
            string Type = StringTable[f.ReadInt16()];
            bytesread += 2;
            AdvancePositionIfNeeded(f.Tell());
            Witcher3Value val = ReadValue(f, Type, false);
            val.Name = StringTable[NameCode];
            val.Size += bytesread;
            return val;
        }

        void AdvancePositionIfNeeded(int startpos)
        {
            if (position < VariableIndex.Count - 1)
            {
                if (VariableIndex[position + 1].Offset == startpos)
                    position++;
            }
        }

        Witcher3Value ReadArray(BinaryReader f, string TypeName, bool sizeCheck)
        {
            ArrayStringResult result = arrayStringReader.Process(TypeName);
            if (result.d1 == "2" && result.d2 == "0")
            {
                int count = f.ReadInt32();
                int bytesread = 4;
                Witcher3Value[] items = new Witcher3Value[count];
                for (int i = 0; i < count; i++)
                {
                    Witcher3Value val = ReadValue(f, result.t3, sizeCheck);
                    bytesread += val.Size;
                    items[i] = val;
                }
                Witcher3Value array = new Witcher3Value();
                array.Value = items;
                array.Size = bytesread;
                return array;
            }
            else if (result.d1 == "154" && result.d2 == "0")
            {
                int bytestoread = f.ReadInt32();
                int bytesread = 4;
                Witcher3GenericVariable var = ReadVariable(f, bytestoread);
                bytesread += var.Size;
                Witcher3Value val = new Witcher3Value();
                val.Value = var;
                val.Size = bytesread;
                return val;
            }
            else
            {
                throw new System.Exception("array type not comprehended");
            }
        }



        void ReadVariableTable(BinaryReader f)
        {
            f.Seek(VariableTableOffset);
            int count = f.ReadInt32();
            VariableIndex = new List<VariableIndexEntry>();
            for (int i = 0; i < count; i++)
            {
                VariableIndexEntry v = new VariableIndexEntry();
                v.Offset = f.ReadInt32();
                v.Size = f.ReadInt32();
                VariableIndex.Add(v);
            }
            VariableIndex = VariableIndex.OrderBy(o => o.Offset).ToList();
        }

        void ReadStringTable(BinaryReader f)
        {
            f.Seek(StringTableFooterOffset);
            NMSectionOffset = f.ReadInt32();
            RBSectionOffset = f.ReadInt32();
            f.Seek(NMSectionOffset);
            if (f.ReadString(2) != "NM")
                throw new InvalidDataException("Corrupt NM Variable");
            StringTableOffset = f.Tell();
            f.Seek(RBSectionOffset);
            if (f.ReadString(2) != "RB")
                throw new InvalidDataException("Corrupt RB Variable");
            int count = f.ReadInt32();
            MainVariableIndex = new List<VariableIndexEntry>();
            for (int i = 0; i < count; i++)
            {
                VariableIndexEntry v = new VariableIndexEntry();
                v.Size = f.ReadInt16();
                v.Offset = f.ReadInt32();
                MainVariableIndex.Add(v);
            }
            f.Seek(StringTableOffset);
            int manuVariableSize = StringTableFooterOffset - StringTableOffset;
            if (f.ReadString(4) != "MANU")
                throw new InvalidDataException("Corrupt MANU Variable");
            int strcount = f.ReadInt32();
            if (f.ReadInt32() != 0)
                throw new InvalidDataException("Expected 0 in MANU");
            int remaining = manuVariableSize - 12;
            StringTable = new List<string>();
            StringTable.Add("");
            for (int i = 0; i < strcount; i++)
            {
                int strsize = f.ReadByte();
                remaining -= 1;
                StringTable.Add(f.ReadString(strsize));
                remaining -= strsize;
            }
            if (f.ReadInt32() != 0)
                throw new InvalidDataException("Expected zero at foot of MANU");
            remaining -= 4;
            if (f.ReadString(4) != "ENOD")
                throw new InvalidDataException("Improper end to MANU");
            remaining -= 4;
            if (remaining != 0)
                throw new InvalidDataException("Incorrect number of bytes read in MANU");
        }

        void ReadHeader(BinaryReader f)
        {
            f.Seek(3084);
            string savstring = f.ReadString(4);
            if (savstring != "SAV3")
                throw new InvalidDataException("Invalid save file");
            VerifyTypecodes(f);
        }

        void ReadFooter(BinaryReader f)
        {
            f.SeekFromEnd(-6);
            VariableTableOffset = f.ReadInt32();
            if (f.ReadString(2) != "SE")
            {
                throw new InvalidDataException("Invalid save file");
            }
            StringTableFooterOffset = VariableTableOffset - 10;
        }

        void VerifyTypecodes(BinaryReader f)
        {
            if (f.ReadInt32() != 64)
            {
                throw new InvalidDataException("Typecode failure");
            }
            int code2 = f.ReadInt32();
            if (!validFileVersions.Contains(code2))
            {
                throw new InvalidDataException("Typecode failure");
            }
            if (f.ReadInt32() != 163)
            {
                throw new InvalidDataException("Typecode failure");
            }

        }

        Witcher3BS FindInHierarchy(string name)
        {
            for (int i = 0; i < Hierarchy.Count; i++)
            {
                if (((Witcher3BS)Hierarchy[i]).Name == name)
                    return Hierarchy[i] as Witcher3BS;
            }
            throw new System.Exception("Variable " + name + " not found");
        }

        public Witcher3HandleData FindLevelManager()
        {
            Witcher3BS WorldInfo = FindInHierarchy("worldInfo");
            string currentWorld = (string)((Witcher3VL)WorldInfo.Members[0]).Value.Value;
            Witcher3BS Universe = FindInHierarchy("universe");
            Witcher3BS Player = Universe.SearchForByName("Player") as Witcher3BS;
            if (Player == null)
                throw new System.Exception("Player not found in Universe");
            string PlayerID = ((Witcher3VL)Player.SearchForByName("id")).Value.Value as string;
            Witcher3BS World = FindNamedCollectionContainingVLValue(Universe, currentWorld, "name");
            if (World == null)
                throw new System.Exception("unable to find world");
            Witcher3BS LayerStorage = World.SearchForByName("layerStorage") as Witcher3BS;
            Witcher3BS EntityData = FindNamedCollectionContainingVLValue(LayerStorage, PlayerID, "idTag");
            Witcher3SS EntityDataSetContainer = GetFirstMemberOfType(EntityData, Witcher3RawType.SS) as Witcher3SS;
            Witcher3SXAP EntityDataSet = EntityDataSetContainer.Members[0] as Witcher3SXAP;
            Witcher3BLCK Entity = EntityDataSet.SearchForByName("Entity") as Witcher3BLCK;
            Witcher3PORP LevelManagerRaw = Entity.SearchForByName("levelManager") as Witcher3PORP;
            Witcher3HandleData LevelData = LevelManagerRaw.Value as Witcher3HandleData;
            return LevelData;
        }

        Witcher3BS FindNamedCollectionContainingVLValue(Witcher3Collection VarToSearch, string query, string attributename)
        {
            foreach (Witcher3GenericVariable v in VarToSearch.Members)
            {
                if (v.Type == Witcher3RawType.BS || v.Type == Witcher3RawType.BLCK)
                {
                    Witcher3Collection candidate = v as Witcher3Collection;
                    Witcher3GenericVariable world = candidate.SearchForByName(attributename);
                    if (world != null)
                    {
                        if ((string)((Witcher3VL)world).Value.Value == query)
                            return candidate as Witcher3BS;
                    }
                }
            }
            return null;
        }

        Witcher3GenericVariable GetFirstMemberOfType(Witcher3Collection collection, Witcher3RawType rawtype)
        {
            foreach (Witcher3GenericVariable v in collection.Members)
            {
                if (v.Type == rawtype)
                    return v;
            }
            return null;
        }

        void GetLevel(Witcher3HandleData LevelData)
        {
            for (int i = 0; i < LevelData.HandleInfo.Count; i++)
            {
                Witcher3Value v = LevelData.HandleInfo[i];
                if (v.Name == "level")
                    CharacterLevel = (int)v.Value;
                if (v.Name == "points")
                {
                    //0 is ability points? 1 is XP
                    Witcher3SimpleSet pointArray = (v.Value as Witcher3Value[])[1] as Witcher3SimpleSet;
                    if (pointArray.SetInfo != null && pointArray.SetInfo.Count > 0)
                    {
                        int FreeUnspentXP = (int)pointArray.SetInfo[0].Value;
                        CharacterFreeXP = FreeUnspentXP;
                    }
                    else CharacterFreeXP = 0;
                }
            }
        }



    }

    public class VariableIndexEntry
    {
        public int Offset;
        public int Size;

        public override string ToString()
        {
            return "(" + Offset.ToString() + ", " + Size.ToString() + ")";
        }
    }

    internal class ArrayStringResult
    {
        public string t1, t2, t3, d1, d2;
    }

    internal class ArrayStringReader
    {
        Dictionary<string, ArrayStringResult> Cache;
        Regex r = new Regex(@"([a-zA-Z]+):(.+)");
        Regex r2 = new Regex(@"([0-9]+),([0-9]+),(.+)");

        public ArrayStringReader()
        {
            Cache = new Dictionary<string, ArrayStringResult>();
        }

        public ArrayStringResult Process(string TypeName)
        {
            if (Cache.Keys.Contains(TypeName))
                return Cache[TypeName];

            Match m = r.Match(TypeName);
            if (!m.Success)
            {
                throw new System.Exception("array string does not match pattern");
            }

            ArrayStringResult result = new ArrayStringResult();
            result.t1 = m.Groups[1].Captures[0].ToString();
            result.t2 = m.Groups[2].Captures[0].ToString();

            Match m2 = r2.Match(result.t2);
            if (!m2.Success)
            {
                throw new System.Exception("array string does not match pattern");
            }

            result.d1 = m2.Groups[1].Captures[0].ToString();
            result.d2 = m2.Groups[2].Captures[0].ToString();
            result.t3 = m2.Groups[3].Captures[0].ToString();
            Cache[TypeName] = result;
            return result;
        }


    }

    public class Witcher3CJournal
    {
        public List<Witcher3JournalEntryStatus> Statuses;

        public Witcher3CJournal(Witcher3BS CJournalManager)
        {
            Statuses = new List<Witcher3JournalEntryStatus>();
            if (CJournalManager.Name != "CJournalManager")
                throw new System.Exception("Not a valid CJournalManager");

            Witcher3BS JActiveEntries = CJournalManager.Members[0] as Witcher3BS;
            if (JActiveEntries.Name != "JActiveEntries")
                throw new System.Exception("Invalid entry to start CJournal");

            Witcher3VL SizeVL = JActiveEntries.Members[0] as Witcher3VL;
            if (SizeVL.Name != "Size")
                throw new System.Exception("Invalid size statement in CJournal");

            uint NumberOfEntries = (uint)SizeVL.Value.Value;
            int i = 1;
            while (i < JActiveEntries.Members.Count)
            {
                Witcher3BS Status = JActiveEntries.Members[i] as Witcher3BS;
                Statuses.Add(ParseStatusVar(Status));
                i++;
            }

        }

        Witcher3JournalEntryStatus ParseStatusVar(Witcher3BS StatusVar)
        {
            Witcher3JournalEntryStatus Status = new Witcher3JournalEntryStatus();
            Witcher3BS PathVar = StatusVar.Members[0] as Witcher3BS;
            if (PathVar.Members.Count % 3 != 0)
                throw new System.Exception("malformed path in SJournalEntryStatus");
            int PathCount = PathVar.Members.Count / 3;
            Status.Path = new Witcher3JournalEntryPath[PathCount];
            int i = 0;
            while (i < PathCount)
            {
                Witcher3JournalEntryPath entrypath = new Witcher3JournalEntryPath();
                entrypath.guid = (string)(PathVar.Members[3 * i] as Witcher3VL).Value.Value;
                entrypath.resource = (string)(PathVar.Members[3 * i + 1] as Witcher3VL).Value.Value;
                Witcher3VL FlagVar = PathVar.Members[3 * i + 2] as Witcher3VL;
                entrypath.flags = (byte)FlagVar.Value.Value;
                Status.Path[i] = entrypath;
                i++;
            }
            string statusstring = ((Witcher3VL)StatusVar.Members[1]).Value.Value as string;
            Status.Status = ParseStatusStateFromString(statusstring);
            Status.Unread = (bool)((Witcher3VL)StatusVar.Members[2]).Value.Value;
            Status.PrimaryGUID = Status.Path.Last().guid;
            return Status;
        }

        FileQuestStatusState ParseStatusStateFromString(string statusstring)
        {
            switch (statusstring)
            {
                case "JS_Success":
                    return FileQuestStatusState.Success;
                case "JS_Failed":
                    return FileQuestStatusState.Failed;
                case "JS_Inactive":
                    return FileQuestStatusState.Inactive;
                case "JS_Active":
                    return FileQuestStatusState.Active;
                default:
                    return FileQuestStatusState.NotFound;
            }
        }
    }

    public class Witcher3JournalEntryStatus
    {
        public Witcher3JournalEntryPath[] Path;
        public FileQuestStatusState Status;
        public bool Unread;
        public string PrimaryGUID;
    }

    public class Witcher3JournalEntryPath
    {
        public string guid;
        public string resource;
        public byte flags;
    }

    public class Witcher3GwentCard
    {
        public int cardIndex, numCopies;
        public string Name;
        public string Location;
        public string AssociatedQuest;

        public Witcher3GwentCard()
        {
            cardIndex = 0;
            numCopies = 0;
            Name = "";
            Location = "";
            AssociatedQuest = "none";
        }


        public Witcher3GwentCard(Witcher3BS SBCollectionCard)
        {
            foreach (Witcher3GenericVariable v in SBCollectionCard.Members)
            {
                Witcher3VL val = v as Witcher3VL;
                if (val.Name == "cardIndex")
                    cardIndex = (int)val.Value.Value;
                else if (val.Name == "numCopies")
                    numCopies = (int)val.Value.Value;
                else throw new System.Exception("unrecognized key in SBCollectionCard");
            }
            Name = "Unknown";
            Location = "Unknown";
            AssociatedQuest = "None";
        }
    }

    public class Witcher3GwentManager
    {
        public List<Witcher3GwentCard> RegularCards;
        public List<Witcher3GwentCard> LeaderCards;

        public Witcher3GwentManager(Witcher3BS GwentBS)
        {
            if (GwentBS.Name != "CR4GwintManager")
                throw new System.Exception("Not a valid CR4GwintManager");

            Witcher3BS SBCollectionCardSize = GwentBS.Members[0] as Witcher3BS;
            if (SBCollectionCardSize.Name != "SBCollectionCardSize")
                throw new System.Exception("Invalid entry to start CR4GwintManager");

            Witcher3VL SizeVL = SBCollectionCardSize.Members[0] as Witcher3VL;
            if (SizeVL.Name != "Size")
                throw new System.Exception("Invalid size statement in SBCollectionCardSize");

            Witcher3BS SBLeaderCollectionCardSize = GwentBS.Members[1] as Witcher3BS;
            if (SBLeaderCollectionCardSize.Name != "SBLeaderCollectionCardSize")
                throw new System.Exception("Invalid entry in CR4GwintManager");

            RegularCards = new List<Witcher3GwentCard>();
            LeaderCards = new List<Witcher3GwentCard>();

            uint NumberOfEntries = (uint)SizeVL.Value.Value;
            int i = 1;
            while (i < SBCollectionCardSize.Members.Count)
            {
                Witcher3BS SBCollectionCard = SBCollectionCardSize.Members[i] as Witcher3BS;
                RegularCards.Add(new Witcher3GwentCard(SBCollectionCard));
                i++;
            }


            SizeVL = SBLeaderCollectionCardSize.Members[0] as Witcher3VL;
            if (SizeVL.Name != "Size")
                throw new System.Exception("Invalid size statement in SBLeaderCollectionCardSize");

            NumberOfEntries = (uint)SizeVL.Value.Value;
            i = 1;
            while (i < SBLeaderCollectionCardSize.Members.Count)
            {
                Witcher3BS SBCollectionCard = SBLeaderCollectionCardSize.Members[i] as Witcher3BS;
                LeaderCards.Add(new Witcher3GwentCard(SBCollectionCard));
                i++;
            }


        }
    }

    public enum Witcher3ReadLevel { Quick, Complete }
}
