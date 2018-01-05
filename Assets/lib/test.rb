species_data = nil
File.open("Pokémon Essentials v17.2 2017-10-15/Data/formspecies.dat"){|f|
	species_data = Marshal.load(f)
}
species = 382
for i in formdata[@species]
	next if !i || i<=0
	pbDexDataOffset(dexdata,i,29)
	megastone = dexdata.fgetw
	if megastone>0 && self.hasItem?(megastone)
		ret = i; break
	end
	if !itemonly
		pbDexDataOffset(dexdata,i,56)
		megamove = dexdata.fgetw
		if megamove>0 && self.hasMove?(megamove)
			ret = i; break
		end
	end
end