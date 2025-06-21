Book book1 = new Book();
Console.WriteLine(Book.BooksCount);

Book book2 = new Book("The Great Book", "Auth1");
Console.WriteLine(Book.BooksCount);

Book book3 = new Book("The Great Book 3", "Auth2", 3.5);
Console.WriteLine(Book.BooksCount);

book3.Checkout();

book3.Rating = 4;
Console.WriteLine(book3.BookInfo);

Library bookLibrary = new Library();
bookLibrary.AddBook(book2);
Console.WriteLine(bookLibrary[0].ToString());

public class Book
{
    private static readonly string _libraryName = "Central Library";

    public Book()
    {
        BookTitle = "Unknown Title";
        Author = "Unknown Author";
        _isAvailable = true;
        _createdDate = DateTime.Now;
        _booksCount += 1;
    }
    public Book(string title, string author) : this()
    {
        BookTitle = title;
        Author = author;
    }

    public Book(string title, string author, double rating) : this(title, author)
    {
        Rating = rating;
    }
    private string _bookTitle;
    public string BookTitle
    {
        get { return _bookTitle; }
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty");
            _bookTitle = value;
        }
    }
    public int BookId { get; set; }
    private DateTime _publishedDate;
    public DateTime PublishedDate
    {
        get
        {
            return _publishedDate;
        }
        set
        {
            if (value > DateTime.Now)
                throw new ArgumentException("Published date can't be in future");
            _publishedDate = value;
        }
    }
    private bool _isAvailable;
    public bool IsAvailable
    {
        get { return _isAvailable; }
        private set { _isAvailable = value; }
    }

    private double _rating;
    public double Rating
    {
        get { return _rating; }
        set
        {
            if (value < MIN_RATING)
                throw new ArgumentException("Rating should be minimum " + MIN_RATING);
            else if (value > MAX_RATING)
                throw new ArgumentException("Rating should be maximum " + MAX_RATING);
            _rating = value;
        }
    }

    private string _author;
    public string Author
    {
        get { return _author; }
        set { _author = value; }
    }

    public string Review { get; set; }
    private readonly DateTime _createdDate;
    public DateTime CreatedDate => _createdDate;

    public string BookInfo
    {
        get { return $"{BookTitle} by {Author} has {Rating} rating and currently {(IsAvailable ? "available" : "Unavailable")}"; }
    }

    public void Checkout()
    {
        if (!_isAvailable)
            throw new ArgumentException("Book is already checked out");
        _isAvailable = false;
        Console.WriteLine($"{BookTitle} has been checked out.");
    }

    public void Checkin()
    {
        if (_isAvailable)
            throw new ArgumentException("Book is already available");
        _isAvailable = true;
        Console.WriteLine($"{BookTitle} has been returned.");
    }

    public void GiveRating(double rating)
    {
        Rating = rating;
        Console.WriteLine($"For {BookTitle} - {rating} is given.");
    }
    public void GiveRating(int rating)
    {
        Rating = rating;
        Console.WriteLine($"For {BookTitle} - {rating} is given.");
    }
    public void GiveRating(int rating, string review = null)
    {
        Rating = rating;
        Review = review;
        Console.WriteLine($"For {BookTitle} - {rating} is given.");
    }
    public string GetBookInfo()
    {
        return BookInfo;
    }
    public bool TryFindBook(string title, out Book foundBook)
    {
        if (_bookTitle == title)
        {
            foundBook = this;
            return true;
        }

        foundBook = null;
        return false;
    }
    public bool UpdateBookRating(ref Book book, double newRating)
    {
        book.Rating = newRating;
        return true;
    }

    private static int _booksCount = 0;
    public static int BooksCount
    {
        get
        {
            return _booksCount;
        }
    }

    public static void DisplayLibraryStats()
    {
        Console.WriteLine($"Library: {_libraryName}");
        Console.WriteLine($"Total Books Created: {_booksCount}");
    }
    // Demonstrate difference
    public void ShowStaticVsInstance()
    {
        Console.WriteLine($"Instance: {this.BookTitle}"); // Instance member
        Console.WriteLine($"Static: {Book.BooksCount}");   // Static member
    }

    public override string ToString()
    {
        return this.BookInfo;
    }

    public const double MIN_RATING = 0;
    public const double MAX_RATING = 5;

    public enum BookGenre { Fiction, NonFiction, Science, History, Biography }

    public BookMetadata Metadata { get; set; }

    public class BookMetadata
    {
        public string Publisher { get; set; }
        public string ISBN { get; set; }

        public override string ToString()
        {
            return $"Publisher: {Publisher}, ISBN: {ISBN}";
        }
    }
}

public class Library
{
    private List<Book> _books = new List<Book>();

    public Book this[int index]
    {
        get
        {
            if (index < 0 || index >= _books.Count)
                throw new IndexOutOfRangeException("Invalid book index");
            return _books[index];
        }

        set
        {
            if (index < 0 || index > _books.Count)
                throw new IndexOutOfRangeException("Invalid book index");

            _books[index] = value;
        }
    }

    public void AddBook(Book book)
    {
        _books.Add(book);
    }

    public int Count => _books.Count;
}

