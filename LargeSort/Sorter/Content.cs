namespace Sorter;

public record Content(int Number, string String){
    public override string ToString()
    {
        return $"{Number}.{String}";
    }

    public static Content Parse(string? line){
        int i;
        for(i = 0; i < line.Length; i++){
            if(line[i]=='.'){
                break;
            }
        }
        var n = line[..i];
        var s = line[(i+1)..];

        return new Content(int.Parse(n), s);
    }
}