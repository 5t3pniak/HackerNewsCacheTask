namespace HackerNewsCachingService;

public record BestStory(
    string Title,
    string Uri,
    string PostedBy,
    DateTime Time,
    int Score,
    int CommentCount
    );