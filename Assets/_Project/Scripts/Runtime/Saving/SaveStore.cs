using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace QuaThangNayDa
{
    public sealed class SaveStore
    {
        [Serializable] private sealed class Envelope { public int version = 1; public string payload, checksum; }
        private readonly string _directory;
        public SaveStore(string directory) { _directory = Path.GetFullPath(directory); }
        public string SlotPath(int slot)
        {
            if (slot < 0 || slot > 3) throw new ArgumentOutOfRangeException(nameof(slot));
            return Path.Combine(_directory, slot == 0 ? "autosave.json" : $"slot-{slot}.json");
        }
        public bool Exists(int slot) => File.Exists(SlotPath(slot)) || File.Exists(SlotPath(slot) + ".bak");
        private static string Hash(string text)
        {
            using (var hash = SHA256.Create()) return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        private static SaveGame Read(string path)
        {
            if (new FileInfo(path).Length > 8 * 1024 * 1024) throw new InvalidDataException("Save too large.");
            var envelope = JsonUtility.FromJson<Envelope>(File.ReadAllText(path, Encoding.UTF8));
            if (envelope == null || envelope.version != 1 || envelope.payload == null || Hash(envelope.payload) != envelope.checksum)
                throw new InvalidDataException("Save checksum/version failed.");
            var save = JsonUtility.FromJson<SaveGame>(envelope.payload);
            if (save == null) throw new InvalidDataException("Empty save.");
            save.Validate();
            return save;
        }
        public SaveGame Load(int slot, out bool usedBackup)
        {
            usedBackup = false;
            string path = SlotPath(slot);
            try { return Read(path); }
            catch (Exception primary) when (primary is IOException || primary is ArgumentException || primary is UnauthorizedAccessException)
            {
                try { var backup = Read(path + ".bak"); usedBackup = true; return backup; }
                catch (Exception second) when (second is IOException || second is ArgumentException || second is UnauthorizedAccessException)
                { throw new InvalidDataException("Khong tai duoc o luu hoac ban du phong. Tien trinh hien tai duoc giu nguyen.", second); }
            }
        }
        public void Write(int slot, SaveGame save)
        {
            save.Validate();
            if (slot == 0 && QuaThangNayDa.Core.MorningSimulation.Restore(save.Simulation).Ended)
                throw new InvalidOperationException("Khong ghi de autosave an toan bang luot da ket thuc.");
            string path = SlotPath(slot);
            Directory.CreateDirectory(_directory);
            string payload = JsonUtility.ToJson(save);
            string json = JsonUtility.ToJson(new Envelope { payload = payload, checksum = Hash(payload) }, true);
            string temporary = path + ".tmp";
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            { stream.Write(bytes, 0, bytes.Length); stream.Flush(true); }
            Read(temporary); // Validate the whole snapshot before touching the previous good save.
            if (File.Exists(path))
            {
                bool validPrimary;
                try { Read(path); validPrimary = true; }
                catch (Exception ex) when (ex is IOException || ex is ArgumentException) { validPrimary = false; }
                // Never replace the known-good backup with an unreadable primary.
                File.Replace(temporary, path, validPrimary ? path + ".bak" : null);
            }
            else File.Move(temporary, path);
            if (!File.Exists(path + ".bak")) File.Copy(path, path + ".bak");
        }
    }
}
