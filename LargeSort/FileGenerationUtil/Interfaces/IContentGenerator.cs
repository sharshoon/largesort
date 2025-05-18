namespace FileGenerationUtil.Interfaces;

public interface IContentGenerator<T>{
    T GetNext();
}