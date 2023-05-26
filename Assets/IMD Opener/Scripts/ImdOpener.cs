using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImdOpener : MonoBehaviour
{
    public string filePath;
    public bool readFileDebug;
    public bool saveFileDebug;
    byte[] imdFile;
    public IMD currentIMD;

    private void FixedUpdate()
    {
        if (readFileDebug)
        {
            readFileDebug = false;
            LoadIMDFile();
        }
        if (saveFileDebug)
        {
            saveFileDebug = false;
            SaveIMGFile();
        }
    }

    public void LoadIMDFile()
    {
        //Setup
        ReadFileIntoByte();

        if (imdFile == null)
        {
            Debug.Log("IMD Opener: No File, Error");
            return;
        }

        //Header
        byte[] byteHeader = new byte[29];

        for (int i = 0; i < byteHeader.Length; i++)
        {
            byteHeader[i] = imdFile[i];
        }

        currentIMD.header = System.Text.Encoding.ASCII.GetString(byteHeader);

        //Comment
        List<byte> commentBytes = new List<byte>();

        int e = 29;

        while (true)
        {
            //Check for ASCII Terminator
            if (imdFile[e] == 26)
            {
                e++;
                break;
            }
            else
            {
                commentBytes.Add(imdFile[e]);
            }
            e++;
        }

        currentIMD.comment = System.Text.Encoding.ASCII.GetString(commentBytes.ToArray());

        //Track Reader
        currentIMD.tracks = new List<IMDTrack>();

        while (true)
        {
            if (e >= imdFile.Length)
            {
                break;
            }
            //Track Header
            IMDTrack currentTrack = new IMDTrack();
            currentTrack.modeValue = imdFile[e];
            e++;
            currentTrack.cylinder = imdFile[e];
            e++;
            currentTrack.head = imdFile[e];
            e++;
            currentTrack.noSectorsInTrack = imdFile[e];
            e++;
            currentTrack.sectorSize = imdFile[e];
            e++;

            //Sector Numbering Map
            currentTrack.numberingMap = new byte[currentTrack.noSectorsInTrack];
            for (int k = 0; k < currentTrack.numberingMap.Length; k++)
            {
                currentTrack.numberingMap[k] = imdFile[e];
                e++;
            }

            //Sector Cylinder Map (Optional)
            if (GetBit(currentTrack.head, 7))
            {
                currentTrack.cylinderMap = new byte[currentTrack.noSectorsInTrack];
                for (int k = 0; k < currentTrack.cylinderMap.Length; k++)
                {
                    currentTrack.cylinderMap[k] = imdFile[e];
                    e++;
                }
            }

            //Sector Head Map (Optional)
            if (GetBit(currentTrack.head, 6))
            {
                currentTrack.headMap = new byte[currentTrack.noSectorsInTrack];
                for (int k = 0; k < currentTrack.headMap.Length; k++)
                {
                    currentTrack.headMap[k] = imdFile[e];
                    e++;
                }
            }

            //Sector Data Records
            currentTrack.sectors = new IMDSectorData[currentTrack.noSectorsInTrack];
            for (int k = 0; k < currentTrack.sectors.Length; k++)
            {
                //Data Type
                currentTrack.sectors[k].dataType = imdFile[e];
                e++;

                if (currentTrack.sectors[k].dataType != 0)
                {
                    if (currentTrack.sectors[k].dataType % 2 == 0)
                    {
                        //Compressed Data
                        currentTrack.sectors[k].compressedValue = imdFile[e];
                        e++;
                    }
                    else
                    {
                        //Data
                        switch (currentTrack.sectorSize)
                        {
                            case 0:
                                currentTrack.sectors[k].data = new byte[128];
                                break;
                            case 1:
                                currentTrack.sectors[k].data = new byte[256];
                                break;
                            case 2:
                                currentTrack.sectors[k].data = new byte[512];
                                break;
                            case 3:
                                currentTrack.sectors[k].data = new byte[1024];
                                break;
                            case 4:
                                currentTrack.sectors[k].data = new byte[2048];
                                break;
                            case 5:
                                currentTrack.sectors[k].data = new byte[4096];
                                break;
                            case 6:
                                currentTrack.sectors[k].data = new byte[8192];
                                break;
                            default:
                                Debug.LogError("IMD Opener: Sector Size outside of range 0-6 - '" + currentTrack.sectorSize + "'");
                                break;
                        }

                        for (int p = 0; p < currentTrack.sectors[k].data.Length; p++)
                        {
                            currentTrack.sectors[k].data[p] = imdFile[e];
                            e++;
                        }
                    }
                }
            }

            //Final
            currentIMD.tracks.Add(currentTrack);
        }

        //Debug
        ReadAllContents();
    }

    public void ReadFileIntoByte()
    {
        imdFile = File.ReadAllBytes(filePath);
    }

    public void ReadAllContents()
    {
        Debug.Log("IMD Opener: Header - '" + currentIMD.header + "'");
        Debug.Log("IMD Opener: Comment - '" + currentIMD.comment + "'");
        for (int i = 0; i < currentIMD.tracks.Count; i++)
        {
            Debug.Log("IMD Opener: Reading Track #" + i);
            switch (currentIMD.tracks[i].modeValue)
            {
                case 0:
                    Debug.Log("IMD Opener: Mode Value - '500' kbps (FM)");
                    break;
                case 1:
                    Debug.Log("IMD Opener: Mode Value - '300' kbps (FM)");
                    break;
                case 2:
                    Debug.Log("IMD Opener: Mode Value - '250' kbps (FM)");
                    break;
                case 3:
                    Debug.Log("IMD Opener: Mode Value - '500' kbps (MFM)");
                    break;
                case 4:
                    Debug.Log("IMD Opener: Mode Value - '300' kbps (MFM)");
                    break;
                case 5:
                    Debug.Log("IMD Opener: Mode Value - '250' kbps (MFM)");
                    break;
                default:
                    Debug.LogError("IMD Opener: Mode Value outside of range 0-5 - '" + currentIMD.tracks[i].modeValue + "'");
                    break;
            }
            Debug.Log("IMD Opener: Cylinder - '" + currentIMD.tracks[i].cylinder + "'");
            if (!GetBit(currentIMD.tracks[i].head, 0))
            {
                Debug.Log("IMD Opener: Head - '0'");
            }
            else
            {
                Debug.Log("IMD Opener: Head - '1'");
            }
            if (GetBit(currentIMD.tracks[i].head, 6))
            {
                Debug.Log("IMD Opener: Track has Sector Head Map");
            }
            if (GetBit(currentIMD.tracks[i].head, 7))
            {
                Debug.Log("IMD Opener: Track has Sector Cylinder Map");
            }
            Debug.Log("IMD Opener: # Sectors In Track - '" + currentIMD.tracks[i].noSectorsInTrack + "'");
            switch (currentIMD.tracks[i].sectorSize)
            {
                case 0:
                    Debug.Log("IMD Opener: Sector Size - '128' bytes/sector");
                    break;
                case 1:
                    Debug.Log("IMD Opener: Sector Size - '256' bytes/sector");
                    break;
                case 2:
                    Debug.Log("IMD Opener: Sector Size - '512' bytes/sector");
                    break;
                case 3:
                    Debug.Log("IMD Opener: Sector Size - '1024' bytes/sector");
                    break;
                case 4:
                    Debug.Log("IMD Opener: Sector Size - '2048' bytes/sector");
                    break;
                case 5:
                    Debug.Log("IMD Opener: Sector Size - '4096' bytes/sector");
                    break;
                case 6:
                    Debug.Log("IMD Opener: Sector Size - '8192' bytes/sector");
                    break;
                default:
                    Debug.LogError("IMD Opener: Sector Size outside of range 0-6 - '" + currentIMD.tracks[i].sectorSize + "'");
                    break;
            }
            for (int e = 0; e < currentIMD.tracks[i].sectors.Length; e++)
            {
                switch (currentIMD.tracks[i].sectors[e].dataType)
                {
                    case 0:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Sector Data Unavailable");
                        break;
                    case 1:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Normal Data");
                        break;
                    case 2:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Normal Data (Compressed)");
                        break;
                    case 3:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Data w/ 'Deleted-Data' Address Mark");
                        break;
                    case 4:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Data w/ 'Deleted-Data' Address Mark (Compressed)");
                        break;
                    case 5:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Read Error Data");
                        break;
                    case 6:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Read Error Data (Compressed)");
                        break;
                    case 7:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Deleted Data");
                        break;
                    case 8:
                        Debug.Log("IMD Opener: Track " + i + " Sector " + e + " Data Type - Deleted Data (Compressed)");
                        break;
                    default:
                        Debug.LogError("IMD Opener: Track " + i + " Sector " + e + " Data Type outside of range 0-8 - '" + currentIMD.tracks[i].sectors[e].dataType + "'");
                        break;
                }
            }
        }
    }

    public void SaveIMGFile()
    {
        //Count Bytes
        int bytesTotal = 0;

        for (int i = 0; i < currentIMD.tracks.Count; i++)
        {
            switch (currentIMD.tracks[i].sectorSize)
            {
                case 0:
                    bytesTotal += 128 * currentIMD.tracks[i].sectors.Length;
                    break;
                case 1:
                    bytesTotal += 256 * currentIMD.tracks[i].sectors.Length;
                    break;
                case 2:
                    bytesTotal += 512 * currentIMD.tracks[i].sectors.Length;
                    break;
                case 3:
                    bytesTotal += 1024 * currentIMD.tracks[i].sectors.Length;
                    break;
                case 4:
                    bytesTotal += 2048 * currentIMD.tracks[i].sectors.Length;
                    break;
                case 5:
                    bytesTotal += 4096 * currentIMD.tracks[i].sectors.Length;
                    break;
                case 6:
                    bytesTotal += 8192 * currentIMD.tracks[i].sectors.Length;
                    break;
                default:
                    break;
            }
        }
        byte[] bytes = new byte[bytesTotal];
        int index = 0;

        //Save Data
        for (int i = 0; i < currentIMD.tracks.Count; i++)
        {
            int sectorSizeBytes = 0;
            switch (currentIMD.tracks[i].sectorSize)
            {
                case 0:
                    sectorSizeBytes = 128;
                    break;
                case 1:
                    sectorSizeBytes = 256;
                    break;
                case 2:
                    sectorSizeBytes = 512;
                    break;
                case 3:
                    sectorSizeBytes = 1024;
                    break;
                case 4:
                    sectorSizeBytes = 2048;
                    break;
                case 5:
                    sectorSizeBytes = 4096;
                    break;
                case 6:
                    sectorSizeBytes = 8192;
                    break;
                default:
                    break;
            }

            for (int e = 0; e < currentIMD.tracks[i].sectors.Length; e++)
            {
                if (currentIMD.tracks[i].sectors[e].dataType != 0)
                {
                    if (currentIMD.tracks[i].sectors[e].dataType % 2 == 0)
                    {
                        //Compressed Data
                        for (int v = 0; v < sectorSizeBytes; v++)
                        {
                            bytes[index] = currentIMD.tracks[i].sectors[e].compressedValue;
                            index++;
                        }
                    }
                    else
                    {
                        //Regular Data
                        for (int p = 0; p < currentIMD.tracks[i].sectors[e].data.Length; p++)
                        {
                            try
                            {
                                bytes[index] = currentIMD.tracks[i].sectors[e].data[p];

                            }
                            catch (Exception)
                            {
                                Debug.Log(index + " i " + i + " e " + e + " p " + p);
                                throw;
                            }
                            index++;
                        }
                    }
                }
                else
                {
                    //Unreadable Data
                    index += sectorSizeBytes;
                }
            }
        }
        Debug.Log(index);
        string newpath = filePath.Substring(0, filePath.Length - 3) + "IMG";
        File.WriteAllBytes(newpath, bytes);
    }

    bool GetBit(byte b, int bitNumber)
    {
        System.Collections.BitArray ba = new BitArray(new byte[] { b });
        return ba.Get(bitNumber);
    }
}

[System.Serializable]
public struct IMD
{
    public string header;
    public string comment;
    public List<IMDTrack> tracks;
}

[System.Serializable]
public struct IMDTrack
{
    public byte modeValue;
    public byte cylinder;
    public byte head;
    public byte noSectorsInTrack;
    public byte sectorSize;
    public byte[] numberingMap;
    public byte[] cylinderMap;
    public byte[] headMap;
    public IMDSectorData[] sectors;

}

[System.Serializable]
public struct IMDSectorData
{
    public byte dataType;
    public byte[] data;
    public byte compressedValue;
}

