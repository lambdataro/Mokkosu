type Color = Red | Blue(Int) | Green | RGB(Int, Int, Int -> Int -> Int)
and Tree<T,U> = Leaf | Node(T, Tree<T>, Tree<T>);

do 123;
do 3.14;
do 3e10;
do "hello, world.";
do '3';
do '\n';
do "hello,\nworld.";
do @"hello\nworld.";

do Red;
do Green (3, 4, 5);
