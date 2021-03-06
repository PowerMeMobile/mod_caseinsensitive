
GMCS = gmcs
PUB = pub
BIN = bin
CS_FLAGS = -target:library -r:System.dll -r:System.Web.dll -r:System.Configuration.dll
MKDIR = mkdir -p
RMDIR = rmdir

DLL = RG.ModCaseInsensitive.dll
SRC = src/RG.ModCaseInsensitive

all: $(BIN)/$(DLL)

$(BIN):
	$(MKDIR) $(BIN)

$(BIN)/$(DLL): $(BIN) $(SRC)/*.cs
	$(GMCS) $(CS_FLAGS) -out:$(BIN)/$(DLL) $(SRC)/*.cs

clean: $(BIN)
	$(RM) $(BIN)/$(DLL)
	$(RMDIR) $(BIN)

