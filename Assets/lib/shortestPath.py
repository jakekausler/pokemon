import networkx as nx


def makeGraph(nodes, edges):
    G = nx.Graph()
    G.add_nodes_from(nodes)
    G.add_edges_from(edges)
    return G


def cellNumberFromRowCell(row, cell, width):
    return row * width + cell


with open("input") as f:
    m = []
    for line in f.readlines():
        line = line.replace("\n", "")
        if line != "":
            r = []
            for c in line:
                r.append(c)
            m.append(r)
    nodes = []
    valid = []
    edges = []
    i = 0
    for row in range(len(m)):
        width = len(m[row])
        for cell in range(width):
            currLocation = cellNumberFromRowCell(row, cell, width)
            nodes.append(currLocation)
            if m[row][cell] != "#":
                valid.append(i)
                if m[row][cell + 1] != "#":
                    edges.append(
                        [
                            currLocation,
                            cellNumberFromRowCell(row, cell + 1, width)
                        ]
                    )
                if m[row][cell - 1] != "#":
                    edges.append(
                        [
                            currLocation,
                            cellNumberFromRowCell(row, cell - 1, width)
                        ]
                    )
                if m[row + 1][cell] != "#":
                    edges.append(
                        [
                            currLocation,
                            cellNumberFromRowCell(row + 1, cell, width)
                        ]
                    )
                if m[row - 1][cell] != "#":
                    edges.append(
                        [
                            currLocation,
                            cellNumberFromRowCell(row - 1, cell, width)
                        ]
                    )
            i += 1

    validDict = {}
    for i in range(len(valid)):
        validDict[valid[i]] = i
    for e in range(len(edges)):
        for v in valid:
            if edges[e][0] == v:
                edges[e][0] = validDict[v]
            if edges[e][1] == v:
                edges[e][1] = validDict[v]
    valid = [i for i in range(len(valid))]
    graph = makeGraph(valid, edges)
    print edges
    shortestPaths = {}
    i = 1
    count = len(validDict.keys())
    for key in validDict:
        print i, "of", count
        i += 1
        for key2 in validDict:
            try:
                shortestPaths[(key, key2)] = nx.shortest_path(graph, validDict[key], validDict[key2])
            except Exception as e:
                pass
    print shortestPaths
    print shortestPaths[(7152, 7139)]
