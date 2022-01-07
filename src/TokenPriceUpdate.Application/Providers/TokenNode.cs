using System.Collections.Generic;
using System.Linq;

namespace TokenPriceUpdate.Providers
{
    public class TokenNode
    {
        public static string[] RootTokenNodeNames { get; set; }
        public const int InfinitePathWeight = int.MaxValue - 1;
        public const int MaxTracePath = 3;

        public TokenNode(string tokenSymbol, bool isRootToken = false)
        {
            TokenSymbol = tokenSymbol;
            TargetNodeInfo = new Dictionary<string, (string, int)>();
            foreach (var rootTokenNodeName in RootTokenNodeNames)
            {
                TargetNodeInfo.TryAdd(rootTokenNodeName, (string.Empty, InfinitePathWeight));
            }

            if (isRootToken)
            {
                TargetNodeInfo[tokenSymbol] = (string.Empty, 0);
            }
        }

        public string TokenSymbol { get; }
        public Dictionary<string, TokenNode> Neighbours { get; } = new();
        public Dictionary<string, (string, int)> TargetNodeInfo { get; }

        public void AddNeighbour(TokenNode neighbour)
        {
            if (Neighbours.TryGetValue(neighbour.TokenSymbol, out _))
            {
                return;
            }

            Neighbours.TryAdd(neighbour.TokenSymbol, neighbour);
            neighbour.Neighbours.TryAdd(TokenSymbol, this);

            TryUpdateVerticesPathWeight(this, neighbour);
        }

        public static void TryUpdateVerticesPathWeight(TokenNode originVertices, TokenNode targetVertices)
        {
            foreach (var rootToken in RootTokenNodeNames)
            {
                TryUpdateVerticesPathWeight(originVertices, targetVertices, rootToken);
            }
        }

        private static void TryUpdateVerticesPathWeight(TokenNode originVertices,
            TokenNode targetVertices, string rootToken)
        {
            var (_, targetPath) = targetVertices.TargetNodeInfo[rootToken];
            var (_, originTargetNodePath) = originVertices.TargetNodeInfo[rootToken];

            if (originTargetNodePath == targetPath)
            {
                return;
            }

            if (originTargetNodePath < targetPath)
            {
                if (originTargetNodePath >= MaxTracePath)
                {
                    return;
                }

                TryUpdateVerticesPathWeight(targetVertices, originVertices, rootToken);
                return;
            }

            if (targetPath >= MaxTracePath || originTargetNodePath == targetPath + 1)
            {
                return;
            }

            var targetSymbol = targetVertices.TokenSymbol;
            originVertices.TargetNodeInfo[rootToken] = (targetSymbol, targetPath + 1);
            foreach (var neighbour in originVertices.Neighbours.Values.Where(neighbour =>
                neighbour.TokenSymbol != targetSymbol))
            {
                TryUpdateVerticesPathWeight(neighbour, originVertices, rootToken);
            }
        }
    }
}