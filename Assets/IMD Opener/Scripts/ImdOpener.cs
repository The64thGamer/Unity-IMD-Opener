using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImdOpener : MonoBehaviour
{
    public string filePath;
    public bool readFileDebug;
    byte[] imdFile;
    IMD currentIMD;

    private void FixedUpdate()
    {
        if(readFileDebug)
        {
            readFileDebug = false;
            LoadIMDFile();
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
        int j = 0;
        currentIMD.tracks = new List<IMDTrack>();

        for (int i = 0; i < 1; i++)
        {
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
            if (GetBit(currentIMD.tracks[i].head, 7))
            {
                currentTrack.cylinderMap = new byte[currentTrack.noSectorsInTrack];
                for (int k = 0; k < currentTrack.cylinderMap.Length; k++)
                {
                    currentTrack.cylinderMap[k] = imdFile[e];
                    e++;
                }
            }

            //Sector Head Map (Optional)
            if (GetBit(currentIMD.tracks[i].head, 6))
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

                //Data
                currentTrack.sectors[k].data = new byte[(currentTrack.sectorSize + 1) * 128];
                for (int p = 0; p < currentTrack.sectors[k].data.Length; p++)
                {
                    currentTrack.sectors[k].data[p] = imdFile[e];
                    e++;
                }
            }

            //Final
            currentIMD.tracks.Add(currentTrack);
            j++;
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
            if (!GetBit(currentIMD.tracks[i].head,0))
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
        }
    }
    bool GetBit(byte b, int bitNumber)
    {
        System.Collections.BitArray ba = new BitArray(new byte[] { b });
        return ba.Get(bitNumber);
    }
}

public struct IMD
{
    public string header;
    public string comment;
    public List<IMDTrack> tracks;
}

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
public struct IMDSectorData
{
    public byte dataType;
    public byte[] data;
}

