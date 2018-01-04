import networkx as nx
import itertools as it


class BoatMover:
    def __init__(
        self,
        characters,
        exclusions,
        minInBoat,
        maxInBoat,
        numberOfAreas,
        mustBeInBoat,
        characterWidth=4,
    ):
        self.characters = characters
        self.exclusions = exclusions
        self.minInBoat = minInBoat
        self.maxInBoat = maxInBoat
        self.numberOfAreas = numberOfAreas
        self.nodes = []
        self.edges = []
        self.states = []
        self.graph = None
        self.characterWidth = characterWidth
        self.mustBeInBoat = mustBeInBoat

    def createGraph(self):
        print "Creating graph..."
        G = nx.Graph()
        G.add_nodes_from(self.nodes)
        G.add_edges_from(self.edges)
        self.graph = G
        print "...Created graph"

    def printStates(self):
        areas = []
        for state in self.states:
            for area in state:
                if len(areas) <= area[0]:
                    areas.append([])
                areas[area[0]].append(area[1])
        st = ""
        for a in range(len(areas)):
            st += str(a).ljust(self.characterWidth * len(self.characters) + 1)
        print st
        for s in range(len(areas[0])):
            st = ""
            for a in range(len(areas)):
                st += ",".join(areas[a][s]).ljust(
                    self.characterWidth *
                    len(self.characters) + 1
                )
            print st

    def containsAllCharacters(self, state):
        s = set()
        for area in state:
            s = s.union(area[1])
        return self.characters == s

    def validArea(self, area):
        checks = []
        for exc in self.exclusions:
            pres = []
            nots = []
            for ch in exc:
                if ch[0] == "!":
                    nots.append(ch[1:])
                else:
                    pres.append(ch)
            checks.append((pres, nots))
        for check in checks:
            pres = check[0]
            nots = check[1]
            if (
                all(p in area for p in pres) and
                all(n not in area for n in nots)
            ):
                return False
        return True

    def removeInvalidStates(self):
        print "Removing invalid states..."
        states = []
        for state in self.states:
            if self.containsAllCharacters(state):
                states.append(state)
        self.states = states
        print "...Removed invalid states"

    def formStates(self):
        print "Forming states..."
        self.states = self.formStatesHelper(self.characters)
        self.removeInvalidStates()
        self.getNodesFromStates()
        self.getEdgesFromStates()
        print "...Formed states"

    def getNodesFromStates(self):
        print "Forming nodes..."
        self.nodes = [i for i in range(len(self.states))]
        print "...Formed nodes"

    def formStatesHelper(self, characters, currentArea=0):
        if currentArea >= self.numberOfAreas:
            return None
        states = []
        for i in range(0, len(characters) + 1):
            for state in it.combinations(characters, i):
                states.append(set(state))
        retVal = []
        for state in states:
            if self.validArea(state):
                state = (currentArea, state)
                diff = characters - state[1]
                if currentArea <= 1:
                    print state
                newStates = self.formStatesHelper(diff, currentArea + 1)
                if newStates is not None:
                    for s in newStates:
                        retVal.append([state] + s)
                else:
                    retVal.append([state])
        return retVal

    def isValidTransition(self, s1, s2):
        for area in range(len(s1)-1):
            diff1 = s1[area][1] & s2[area + 1][1]
            diff2 = s1[area + 1][1] & s2[area][1]
            if (
                len(diff1) >= self.minInBoat and
                len(diff1) <= self.maxInBoat and
                all(i in diff1 for i in self.mustBeInBoat) and
                len(diff2) == 0
            ):
                return True
        return False

    def getEdgesFromStates(self):
        print "Getting Edges..."
        edges = []
        for state in range(len(self.states)):
            if state % 10 == 0:
                print "Working on state", state, "of", len(self.states)
            for state2 in range(len(self.states)):
                if state != state2:
                    if self.isValidTransition(
                        self.states[state],
                        self.states[state2]
                    ):
                        edges.append((state, state2))
                        edges.append((state2, state))
        self.edges = edges
        print "...Got Edges"

    def printState(self, state):
        for area in state:
            s = str(area[0]).ljust(self.characterWidth)
            for character in self.characters:
                if character not in area[1]:
                    s += " ".ljust(self.characterWidth)
                else:
                    s += character.ljust(self.characterWidth)
            print s

    def printTransitions(self):
        leads = []
        for i in range(0, len(self.nodes)):
            leadsTo = []
            for edge in self.edges:
                if edge[0] == i:
                    leadsTo.append(edge[1])
            leadsTo.sort()
            leads.append((i, leadsTo))
        leads.sort()
        print "\n".join(
            [
                str(l[0]) +
                " -> " +
                ",".join([str(i) for i in l[1]])
                for l in leads
            ]
        )

    def makeGraph(self):
        print "Starting..."
        self.formStates()
        self.createGraph()
        print "...Done"

    def getShortestPath(self):
        print "Getting shortest path..."
        sp = nx.shortest_path(self.graph, len(self.nodes) - 1, 0)
        print "...Got shortest path"
        return sp


# characters = set(["W", "G", "C", "F"])
# exclusions = (("W", "G", "!F"), ("G", "C", "!F"))
# mustBeInBoat = ("F")

# floors = 2
# minInBoat = 1
# maxInBoat = 2
# characterWidth = 4

types = ["T", "Pl", "S", "Pr", "R"]

characters = set(["P"] + [t + "G" for t in types] + [t + "M" for t in types])
print characters
exclusions = []
for i in types:
    for j in types:
        if i != j:
            e = []
            e.append("!" + i + "G")
            e.append(i + "M")
            e.append(j + "G")
            exclusions.append(e)
            e = []
            e.append("!" + i + "M")
            e.append(i + "G")
            e.append(j + "M")
            exclusions.append(e)
print exclusions
mustBeInBoat = ("P")

floors = 4
minInBoat = 2
maxInBoat = 3
characterWidth = 4

bm = BoatMover(
    characters,
    exclusions,
    minInBoat,
    maxInBoat,
    floors,
    mustBeInBoat,
    characterWidth
)
bm.makeGraph()
print bm.getShortestPath()

# plt.subplot(121)
# nx.draw(G, with_labels=True, font_weight='bold')
# plt.show()
