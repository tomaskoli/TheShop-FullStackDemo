"""
TheShop ChromaDB Seed Script
Seeds the vector database with shop documents (T&C, warranty, policies, FAQ)

This script stores documents in the format expected by Semantic Kernel's ChromaMemoryStore.

Usage: 
    $env:OPENAI_API_KEY = "your-api-key"
    python seed-chroma.py

Requirements:
    pip install chromadb openai
"""

import chromadb
from openai import OpenAI
import os
import json

CHROMA_HOST = os.getenv("CHROMA_HOST", "localhost")
CHROMA_PORT = int(os.getenv("CHROMA_PORT", "8100"))
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "")
COLLECTION_NAME = "theshop_documents"
EMBEDDING_MODEL = "text-embedding-3-small"

# Shop documents to be indexed
DOCUMENTS = [
    {
        "id": "warranty-general",
        "text": """
        WARRANTY POLICY
        
        TheShop provides a standard 24-month warranty on all electronic products purchased from our store.
        The warranty covers manufacturing defects and hardware failures under normal use conditions.
        
        What is covered:
        - Manufacturing defects
        - Hardware component failures
        - Battery defects (for first 12 months)
        - Screen defects (dead pixels policy: 5+ dead pixels)
        
        What is NOT covered:
        - Physical damage (drops, liquid damage, cracks)
        - Software issues
        - Accessories (unless specifically stated)
        - Products modified or repaired by unauthorized service centers
        """,
        "metadata": {"type": "warranty", "title": "General Warranty Policy"}
    },
    {
        "id": "warranty-extended",
        "text": """
        EXTENDED WARRANTY OPTIONS
        
        TheShop offers extended warranty plans for additional peace of mind:
        
        1. TheShop Care+ (12 months extension): $99
           - Extends standard warranty by 12 months
           - Includes one accidental damage repair
           - Priority support queue
        
        2. TheShop Care+ Premium (24 months extension): $179
           - Extends standard warranty by 24 months
           - Includes two accidental damage repairs
           - Free battery replacement (once)
           - Priority support and dedicated hotline
        
        Extended warranty must be purchased within 30 days of the original product purchase.
        """,
        "metadata": {"type": "warranty", "title": "Extended Warranty Options"}
    },
    {
        "id": "returns-policy",
        "text": """
        RETURN POLICY
        
        We want you to be completely satisfied with your purchase. If you're not happy, you can return most items within 30 days of delivery.
        
        Return conditions:
        - Product must be in original packaging
        - All accessories and documentation must be included
        - Product must be in resalable condition (no scratches, damage, or signs of use)
        - Proof of purchase required
        
        Non-returnable items:
        - Opened software
        - Personalized/customized products
        - Products marked as final sale
        - Gift cards
        
        Refund processing:
        - Original payment method: 5-7 business days
        - Store credit: Immediate
        - Exchange: Subject to availability
        """,
        "metadata": {"type": "returns", "title": "Return Policy"}
    },
    {
        "id": "shipping-info",
        "text": """
        SHIPPING INFORMATION
        
        TheShop offers multiple shipping options:
        
        Standard Shipping (3-5 business days): FREE for orders over $50, otherwise $5.99
        Express Shipping (1-2 business days): $14.99
        Same Day Delivery (select metro areas): $24.99 (order before 12 PM)
        
        Order tracking:
        - Tracking number sent via email once shipped
        - Real-time tracking available on website and app
        - SMS notifications available (opt-in)
        
        International shipping:
        - Available to 50+ countries
        - Delivery times: 7-14 business days
        - Customs and duties not included in shipping cost
        
        Note: Shipping times may vary during holidays and promotional periods.
        """,
        "metadata": {"type": "shipping", "title": "Shipping Information"}
    },
    {
        "id": "payment-methods",
        "text": """
        PAYMENT METHODS
        
        TheShop accepts the following payment methods:
        
        Credit/Debit Cards:
        - Visa, Mastercard, American Express, Discover
        - 3D Secure authentication for added security
        
        Digital Wallets:
        - Apple Pay
        - Google Pay
        - PayPal
        
        Financing Options:
        - TheShop Credit Card: 0% APR for 12 months on purchases over $500
        - Affirm: Buy now, pay later in 3-12 monthly installments
        - Klarna: Pay in 4 interest-free installments
        
        Gift Cards:
        - TheShop gift cards accepted
        - Can be combined with other payment methods
        
        All transactions are encrypted and PCI-DSS compliant.
        """,
        "metadata": {"type": "payment", "title": "Payment Methods"}
    },
    {
        "id": "terms-conditions",
        "text": """
        TERMS AND CONDITIONS (Summary)
        
        By using TheShop website and services, you agree to the following:
        
        Account Responsibility:
        - You are responsible for maintaining account security
        - One account per person
        - Must be 18+ to create an account
        
        Pricing:
        - Prices are in USD unless otherwise stated
        - Prices subject to change without notice
        - Price errors may result in order cancellation
        
        Intellectual Property:
        - All content on TheShop is owned by TheShop or its licensors
        - No reproduction without written permission
        
        Limitation of Liability:
        - TheShop is not liable for indirect or consequential damages
        - Maximum liability limited to purchase price
        
        Dispute Resolution:
        - Disputes will be resolved through binding arbitration
        - Class action waiver applies
        
        For full terms, visit: theshop.com/terms
        """,
        "metadata": {"type": "terms", "title": "Terms and Conditions"}
    },
    {
        "id": "faq-orders",
        "text": """
        FREQUENTLY ASKED QUESTIONS - Orders
        
        Q: How do I track my order?
        A: Once shipped, you'll receive an email with tracking information. You can also track orders in your account under 'Order History'.
        
        Q: Can I cancel my order?
        A: Orders can be cancelled within 1 hour of placement. After that, you'll need to wait for delivery and request a return.
        
        Q: Can I change my shipping address after ordering?
        A: Address changes are possible within 2 hours of order placement. Contact support immediately.
        
        Q: Why was my order cancelled?
        A: Orders may be cancelled due to: payment issues, out of stock items, or suspected fraud. You'll receive an email with details.
        
        Q: How do I use a promo code?
        A: Enter the promo code at checkout in the 'Promo Code' field. Only one promo code per order.
        """,
        "metadata": {"type": "faq", "title": "FAQ - Orders"}
    },
    {
        "id": "faq-products",
        "text": """
        FREQUENTLY ASKED QUESTIONS - Products
        
        Q: Are all products new and original?
        A: Yes, TheShop only sells 100% authentic, brand new products from authorized distributors.
        
        Q: Do products come with manufacturer warranty?
        A: Yes, all products include the full manufacturer warranty plus TheShop's additional warranty coverage.
        
        Q: Are prices negotiable?
        A: Prices are fixed, but we offer regular promotions, student discounts, and price matching.
        
        Q: Do you price match?
        A: Yes! We match prices from major retailers. Submit a price match request within 14 days of purchase.
        
        Q: Can I reserve a product?
        A: Products cannot be reserved. We recommend using 'Notify Me' for out-of-stock items.
        """,
        "metadata": {"type": "faq", "title": "FAQ - Products"}
    },
    {
        "id": "contact-support",
        "text": """
        CONTACT & SUPPORT
        
        Customer Service Hours:
        - Monday - Friday: 8 AM - 10 PM EST
        - Saturday - Sunday: 9 AM - 6 PM EST
        
        Contact Methods:
        - Phone: 1-800-THE-SHOP (1-800-843-7467)
        - Email: support@theshop.com (24-48 hour response)
        - Live Chat: Available on website during business hours
        - Social Media: @TheShopOfficial (Twitter, Facebook, Instagram)
        
        Self-Service Options:
        - Order tracking: theshop.com/track
        - Returns portal: theshop.com/returns
        - FAQ: theshop.com/help
        
        Store Locations:
        - Find your nearest store: theshop.com/stores
        - In-store pickup available
        """,
        "metadata": {"type": "support", "title": "Contact & Support"}
    },
    {
        "id": "privacy-policy",
        "text": """
        PRIVACY POLICY (Summary)
        
        Data We Collect:
        - Account information (name, email, address)
        - Order history and preferences
        - Device and browsing information
        - Payment information (encrypted)
        
        How We Use Your Data:
        - Process orders and payments
        - Provide customer support
        - Send order updates and shipping notifications
        - Marketing communications (with consent)
        - Improve our services
        
        Your Rights:
        - Access your data
        - Request data deletion
        - Opt-out of marketing
        - Data portability
        
        Data Security:
        - SSL encryption
        - PCI-DSS compliant
        - Regular security audits
        
        We never sell your personal data to third parties.
        
        For full privacy policy: theshop.com/privacy
        """,
        "metadata": {"type": "privacy", "title": "Privacy Policy"}
    }
]


def get_embedding(client: OpenAI, text: str) -> list:
    """Get embedding for a single text using OpenAI."""
    response = client.embeddings.create(
        input=text,
        model=EMBEDDING_MODEL
    )
    return response.data[0].embedding


def main():
    print(f"Connecting to ChromaDB at {CHROMA_HOST}:{CHROMA_PORT}...")
    
    chroma_client = chromadb.HttpClient(host=CHROMA_HOST, port=CHROMA_PORT)
    
    if not OPENAI_API_KEY:
        print("Error: OPENAI_API_KEY environment variable is required")
        print("Set it with: $env:OPENAI_API_KEY = 'your-key'")
        return
    
    openai_client = OpenAI(api_key=OPENAI_API_KEY)
    print(f"Using OpenAI embeddings ({EMBEDDING_MODEL})...")
    
    # Delete existing collection if it exists
    try:
        chroma_client.delete_collection(COLLECTION_NAME)
        print(f"Deleted existing collection: {COLLECTION_NAME}")
    except:
        pass
    
    # Create collection (no embedding function - we'll provide embeddings directly)
    collection = chroma_client.create_collection(
        name=COLLECTION_NAME,
        metadata={"description": "TheShop documents for RAG chatbot"}
    )
    print(f"Created collection: {COLLECTION_NAME}")
    
    # Add documents in Semantic Kernel's expected format
    print("\nSeeding documents...")
    for doc in DOCUMENTS:
        doc_id = doc["id"]
        text = doc["text"].strip()
        description = doc["metadata"]["title"]
        
        # Generate embedding
        embedding = get_embedding(openai_client, text)
        
        # Semantic Kernel ChromaMemoryStore expects metadata in this format
        metadata = {
            "id": doc_id,
            "text": text,
            "description": description,
            "additional_metadata": json.dumps(doc["metadata"])
        }
        
        collection.add(
            ids=[doc_id],
            embeddings=[embedding],
            metadatas=[metadata],
            documents=[text]
        )
        print(f"  + {description}")
    
    print(f"\nAdded {len(DOCUMENTS)} documents to collection '{COLLECTION_NAME}'")
    print("\nChromaDB seeding completed successfully!")
    print("\nYou can verify with:")
    print(f'  Invoke-RestMethod -Uri "http://{CHROMA_HOST}:{CHROMA_PORT}/api/v1/collections" -Method GET')


if __name__ == "__main__":
    main()

