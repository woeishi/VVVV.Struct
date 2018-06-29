using System.Collections.Generic;
using System.Linq;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;

namespace VVVV.Struct.Hosting
{
    public class ConflictManager
    {
        public enum ConflictSolutionKind
        {
            Ignore = 1, Split = 2, Overwrite = 3
        }

        public struct ConflictSolution
        {
            public readonly ConflictSolutionKind SolutionKind;
            public readonly string DeclarationName;
            public readonly string DeclarationBody;
            public ConflictSolution(ConflictSolutionKind kind, string name, string body)
            {
                SolutionKind = kind;
                DeclarationName = name;
                DeclarationBody = body;
            }
        }

        IReadOnlyCollection<IInternalPluginHost> FNodes;
        ConflictUI FUI;
        Declaration FExisting;
        Declaration FConflict;
        Dictionary<string, Declaration> FRegister;

        public ConflictManager(IReadOnlyCollection<IInternalPluginHost> nodes)
        {
            FNodes = nodes;
            FUI = new ConflictUI();
            FUI.SelectionChanged += OnSelectionChanged;
            FUI.SelectionTextChanged += OnSelectionTextChanged;
        }

        private void OnSelectionTextChanged(object sender, string e)
        {
            if (FConflict != null)
            {
                var equalsExisting = e == FExisting.Name;
                var enableSplit = !equalsExisting;
                if (equalsExisting)
                    FUI.ShowDiff();
                else if (enableSplit != FUI.SplitEnabled)
                    FUI.ClearDiff();
                FUI.SplitEnabled = enableSplit;
            }
            FUI.NewBodyEnabled = !FRegister.Keys.Contains(e);
        }

        private void OnSelectionChanged(object sender, string e)
        {
            if (FConflict != null)
            {
                FUI.SetNewDeclaration(FRegister[e]);
                if (e == FExisting.Name)
                    FUI.ShowDiff();

                FUI.SplitEnabled = e != FExisting.Name;
            }
        }

        public ConflictSolution Solve(IPluginHost2 declarer, Declaration newDeclaration, Dictionary<string,Declaration> register)
        {
            SetConflictInfo(newDeclaration, register);
            FUI.SetExistingDeclaration(FExisting);
            FUI.SetNewSelection(FRegister.Keys);
            FUI.SetExistingUsers(GetUsers(FExisting.Name));
            FUI.SetNewUsers(new[] { NiceNodePath(declarer) });

            var kind = (ConflictSolutionKind)FUI.ShowDialog();
            return new ConflictSolution(kind, FUI.NewDeclarationName, FUI.NewDeclarationBody);
        }

        void SetConflictInfo(Declaration newDeclaration, Dictionary<string, Declaration> register)
        {
            FUI.Text = $"Conflicting declarations - {newDeclaration.Name}";
            FExisting = register[newDeclaration.Name];
            FConflict = newDeclaration;
            FRegister = new Dictionary<string, Declaration>();
            FRegister.Add(FConflict.Name, FConflict);
            foreach (var kv in register.Skip(1))
                if (kv.Key != newDeclaration.Name)
                    FRegister.Add(kv.Key, kv.Value);
        }

        IEnumerable<string> GetUsers(string name)
        {
            return FNodes
                .Where(n => ((n.Plugin as PluginContainer).PluginBase as IStructDeclarer)?.Declaration?.Name == name)
                .Select(n => NiceNodePath(n));
        }

        static IEnumerable<INode> AncestorsAndSelf(INode n)
        {
            var i = n;
            while (i!=null)
            {
                yield return i;
                i = i.ParentNode;
                if (i.GetNodeInfo().Name == "super_root")
                    yield break;
            }
        }

        static string NiceNodePath(INode n)
        {
            return AncestorsAndSelf(n)
                .Select(a => a.GetNodeInfo().Name)
                .Aggregate((a, b) => b + " > " + a) + $" ({n.GetID()})";
        }
    }
}
