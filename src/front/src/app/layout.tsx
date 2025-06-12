import React from 'react';
import './globals.css';

export const metadata = {
  title: 'Posts Gallery',
  description: 'Discover amazing content from our community',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body>{children}</body>
    </html>
  );
} 