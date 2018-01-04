import json, csv

with open('PBS/pokemon.txt') as f:
    Pokedex = []
    Pokemon = {}
    lines = f.readlines()
    for line in lines:
        line = line.replace("\n", "")
        if line.startswith('#--'):
            Pokedex.append(Pokemon)
            Pokemon = {}
            continue
        elif line.startswith('['):
            Pokemon['Number'] = line[1:-1]
            continue
        else:
            Pokemon[line[:line.find('=')]] = line[line.find('=')+1:]
            continue
    Pokedex.append(Pokemon)
    Pokemon = {}
    for Pokemon in Pokedex:
        if 'EggMoves' in Pokemon:
            Pokemon['EggMoves'] = Pokemon['EggMoves'].split(',')
        if 'Moves' in Pokemon:
            moves = Pokemon['Moves'].split(',')
            Pokemon['Moves'] = []
            for i in range(0, len(moves), 2):
                Pokemon['Moves'].append({
                    'Level': int(moves[i]),
                    'Move': moves[i+1]
                })
        if 'Evolutions' in Pokemon:
            evolutions = Pokemon['Evolutions'].split(',')
            Pokemon['Evolutions'] = []
            if len(evolutions) > 1:
                for i in range(0, len(evolutions), 3):
                    Pokemon['Evolutions'].append({
                        'Pokemon': evolutions[i],
                        'Method': evolutions[i+1],
                        'Value': evolutions[i+2]
                    })
        if 'Number' in Pokemon:
            Pokemon['Number'] = int(Pokemon['Number'])
        if 'Rareness' in Pokemon:
            Pokemon['Rareness'] = int(Pokemon['Rareness'])
        if 'Happiness' in Pokemon:
            Pokemon['Happiness'] = int(Pokemon['Happiness'])
        if 'Shape' in Pokemon:
            Pokemon['Shape'] = int(Pokemon['Shape'])
        if 'StepsToHatch' in Pokemon:
            Pokemon['StepsToHatch'] = int(Pokemon['StepsToHatch'])
        if 'Weight' in Pokemon:
            Pokemon['Weight'] = float(Pokemon['Weight'])
        if 'Height' in Pokemon:
            Pokemon['Height'] = float(Pokemon['Height'])
        if 'BaseEXP' in Pokemon:
            Pokemon['BaseEXP'] = int(Pokemon['BaseEXP'])
        if 'BattlerAltitude' in Pokemon:
            Pokemon['BattlerAltitude'] = int(Pokemon['BattlerAltitude'])
        if 'BattlerEnemyY' in Pokemon:
            Pokemon['BattlerEnemyY'] = int(Pokemon['BattlerEnemyY'])
        if 'BattlerPlayerY' in Pokemon:
            Pokemon['BattlerPlayerY'] = int(Pokemon['BattlerPlayerY'])
        if 'RegionalNumbers' in Pokemon:
            Pokemon['RegionalNumbers'] = [int(n) for n in Pokemon['RegionalNumbers'].split(',')]
        if 'BaseStats' in Pokemon:
            bs = Pokemon['BaseStats'].split(',')
            Pokemon['BaseStats'] = {}
            Pokemon['BaseStats']['HP'] = int(bs[0])
            Pokemon['BaseStats']['Attack'] = int(bs[1])
            Pokemon['BaseStats']['Defense'] = int(bs[2])
            Pokemon['BaseStats']['SpecialAttack'] = int(bs[3])
            Pokemon['BaseStats']['SpecialDefense'] = int(bs[4])
            Pokemon['BaseStats']['Speed'] = int(bs[5])
        if 'EffortPoints' in Pokemon:
            ep = Pokemon['EffortPoints'].split(',')
            Pokemon['EffortPoints'] = {}
            Pokemon['EffortPoints']['HP'] = int(ep[0])
            Pokemon['EffortPoints']['Attack'] = int(ep[1])
            Pokemon['EffortPoints']['Defense'] = int(ep[2])
            Pokemon['EffortPoints']['SpecialAttack'] = int(ep[3])
            Pokemon['EffortPoints']['SpecialDefense'] = int(ep[4])
            Pokemon['EffortPoints']['Speed'] = int(ep[5])
        if 'Compatibility' in Pokemon:
            Pokemon['Compatibility'] = Pokemon['Compatibility'].split(',')
    with open('../data/pokemon.json', 'w') as of:
        json.dump(Pokedex, of, indent=4, sort_keys=True)

with open('PBS/abilities.txt') as f:
    reader = csv.reader(f, delimiter=',', quotechar='"')
    abilities = []
    for row in reader:
        abilities.append({
            'Id': int(row[0]),
            'Name': row[1],
            'FriendlyName': row[2],
            'Description': row[3]
        })
    with open('../data/abilities.json', 'w') as of:
        json.dump(abilities, of, indent=4, sort_keys=True)

with open('PBS/berryplants.txt') as f:
    reader = csv.reader(f, delimiter=',', quotechar='"')
    plants = []
    for row in reader:
        plants.append({
            'Name': row[0],
            'GrowthRate': int(row[1]),
            'MoistureLoss': int(row[2]),
            'MinYield': int(row[3]),
            'MaxYield': int(row[4])
        })
    with open('../data/berryplants.json', 'w') as of:
        json.dump(plants, of, indent=4, sort_keys=True)

with open('PBS/items.txt') as f:
    reader = csv.reader(f, delimiter=',', quotechar='"')
    items = []
    for row in reader:
        items.append({
            'Id': int(row[0]),
            'Name': row[1],
            'FriendlyName': row[2],
            'PluralName': row[3],
            'Pocket': int(row[4]),
            'Price': int(row[5]),
            'Description': row[6],
            'OutOfBattle': int(row[7]),
            'InBattle': int(row[8]),
            'SpecialItem': int(row[9]),
            'Move': row[10]
        })
    with open('../data/items.json', 'w') as of:
        json.dump(items, of, indent=4, sort_keys=True)

with open('PBS/moves.txt') as f:
    reader = csv.reader(f, delimiter=',', quotechar='"')
    moves = []
    for row in reader:
        moves.append({
            'Id': int(row[0]),
            'Name': row[1],
            'FriendlyName': row[2],
            'Function': row[3],
            'Power': int(row[4]),
            'Type': row[5],
            'Category': row[6],
            'Accuracy': int(row[7]),
            'MaxPP': int(row[8]),
            'EffectChance': int(row[9]),
            'Target': int(row[10]),
            'Priority': int(row[11]),
            'Flags': row[12],
            'Description': row[13]
        })
    with open('../data/moves.json', 'w') as of:
        json.dump(moves, of, indent=4, sort_keys=True)

with open('PBS/types.txt') as f:
    lines = f.readlines()
    types = []
    t = {}
    for line in lines:
        line = line.replace("\n", "")
        if not line:
            types.append(t)
            t = {}
            continue
        elif line.startswith('['):
            t['Id'] = line[1:-1]
            continue
        else:
            t[line[:line.find('=')]] = line[line.find('=') + 1:]
            continue
    if 'Weaknesses' in t:
        t['Weaknesses'] = t['Weaknesses'].split(',')
    if 'Resistances' in t:
        t['Resistances'] = t['Resistances'].split(',')
    if 'Immunities' in t:
        t['Immunities'] = t['Immunities'].split(',')
    if 'IsSpecialType' in t:
        t['IsSpecialType'] = bool(t['IsSpecialType'])
    if 'IsPseudoType' in t:
        t['IsPseudoType'] = bool(t['IsPseudoType'])
    types.append(t)
    with open('../data/types.json', 'w') as of:
        json.dump(types, of, indent=4, sort_keys=True)
